<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'

const canvasRef = ref(null)
const fallbackMode = ref(false)

// Shader variables
let gl = null
let program = null
let animationFrameId = null
let lastRenderTime = 0
let isContextLost = false
let reducedMotion = false

// Uniform locations
let timeLocation = null
let resolutionLocation = null
let pointerLocation = null
let pointerStrengthLocation = null

// State
let startTime = Date.now()
let targetPointer = { x: 0.5, y: 0.5 }
let currentPointer = { x: 0.5, y: 0.5 }

// Performance settings
const TARGET_FPS = 45 // Not strictly 60fps to save power
const FRAME_MIN_TIME = 1000 / TARGET_FPS

const vertexShaderSource = `
  attribute vec2 position;
  void main() {
    gl_Position = vec4(position, 0.0, 1.0);
  }
`

const fragmentShaderSource = `
  precision mediump float;
  uniform float u_time;
  uniform vec2 u_resolution;
  uniform vec2 u_pointer;
  uniform float u_pointerStrength;

  // Pseudo-random noise function
  vec3 hash(vec3 p) {
    p = vec3(dot(p, vec3(127.1, 311.7, 74.7)),
             dot(p, vec3(269.5, 183.3, 246.1)),
             dot(p, vec3(113.5, 271.9, 124.6)));
    return -1.0 + 2.0 * fract(sin(p) * 43758.5453123);
  }

  // Simplex noise 3D
  float noise(vec3 p) {
    vec3 i = floor(p + dot(p, vec3(0.333333)));
    vec3 x0 = p - i + dot(i, vec3(0.166666));
    
    vec3 g = step(x0.yzx, x0.xyz);
    vec3 l = 1.0 - g;
    vec3 i1 = min(g.xyz, l.zxy);
    vec3 i2 = max(g.xyz, l.zxy);

    vec3 x1 = x0 - i1 + vec3(0.166666);
    vec3 x2 = x0 - i2 + vec3(0.333333);
    vec3 x3 = x0 - 1.0 + vec3(0.5);

    vec4 h = max(0.6 - vec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
    vec4 n = h * h * h * h * vec4(dot(hash(i), x0),
                                  dot(hash(i + i1), x1),
                                  dot(hash(i + i2), x2),
                                  dot(hash(i + 1.0), x3));
    return dot(n, vec4(52.0));
  }

  // FBM (Fractal Brownian Motion)
  float fbm(vec3 p) {
    float f = 0.0;
    float w = 0.5;
    for (int i = 0; i < 3; i++) {
      f += w * noise(p);
      p *= 2.0;
      w *= 0.5;
    }
    return f;
  }

  void main() {
    vec2 st = gl_FragCoord.xy / u_resolution.xy;
    st.x *= u_resolution.x / u_resolution.y;

    // Pointer influence
    vec2 pointerDist = st - (u_pointer * vec2(u_resolution.x / u_resolution.y, 1.0));
    float influence = exp(-dot(pointerDist, pointerDist) * 3.0) * u_pointerStrength;

    // Time scaling (slow, calm motion)
    float t = u_time * 0.08;

    // Domain warping
    vec3 q = vec3(fbm(vec3(st * 2.0, t)), fbm(vec3(st * 2.5 + vec2(5.2, 1.3), t * 1.2)), 0.0);
    
    // Add subtle pointer distortion to the warp field
    q.xy -= pointerDist * influence * 0.2;

    vec3 r = vec3(fbm(vec3(st * 1.5 + q.xy * 2.0, t * 1.5)), 
                  fbm(vec3(st * 1.8 + q.xy * 1.5 + vec2(8.3, 2.8), t * 1.1)), 
                  0.0);

    float f = fbm(vec3(st * 2.0 + r.xy * 2.0, t));

    // Base colors: Blue for hero (left), Cyan for portal (right)
    vec3 colorBlue = vec3(0.08, 0.38, 0.88); // #1461e0 approx
    vec3 colorCyan = vec3(0.03, 0.55, 0.68); // #088dae approx
    vec3 colorDark = vec3(0.95, 0.97, 1.0);  // Very light background blend

    // Blend based on X position to match layout (Blue left, Cyan right)
    float horizontalBlend = smoothstep(0.1, 0.9, gl_FragCoord.x / u_resolution.x);
    vec3 fieldColor = mix(colorBlue, colorCyan, horizontalBlend);

    // Final composition
    vec3 finalColor = mix(colorDark, fieldColor, f * 0.8 + 0.2);
    
    // Add subtle brightness around pointer
    finalColor += fieldColor * influence * 0.3;

    // Set alpha to 1.0. Overall opacity is controlled by CSS.
    gl_FragColor = vec4(finalColor, 1.0);
  }
`

function compileShader(type, source) {
  const shader = gl.createShader(type)
  gl.shaderSource(shader, source)
  gl.compileShader(shader)

  if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
    console.warn('EduShaderBackground: Shader compilation failed', gl.getShaderInfoLog(shader))
    gl.deleteShader(shader)
    return null
  }
  return shader
}

function initWebGL() {
  if (!canvasRef.value) return false

  gl = canvasRef.value.getContext('webgl', { alpha: false, depth: false, antialias: false })
  if (!gl) {
    console.warn('EduShaderBackground: WebGL not supported')
    return false
  }

  const vs = compileShader(gl.VERTEX_SHADER, vertexShaderSource)
  const fs = compileShader(gl.FRAGMENT_SHADER, fragmentShaderSource)

  if (!vs || !fs) return false

  program = gl.createProgram()
  gl.attachShader(program, vs)
  gl.attachShader(program, fs)
  gl.linkProgram(program)

  if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
    console.warn('EduShaderBackground: Program linking failed', gl.getProgramInfoLog(program))
    gl.deleteProgram(program)
    return false
  }

  gl.useProgram(program)

  // Set up geometry
  const buffer = gl.createBuffer()
  gl.bindBuffer(gl.ARRAY_BUFFER, buffer)
  gl.bufferData(
    gl.ARRAY_BUFFER,
    new Float32Array([-1.0, -1.0, 1.0, -1.0, -1.0, 1.0, -1.0, 1.0, 1.0, -1.0, 1.0, 1.0]),
    gl.STATIC_DRAW,
  )

  const positionLocation = gl.getAttribLocation(program, 'position')
  gl.enableVertexAttribArray(positionLocation)
  gl.vertexAttribPointer(positionLocation, 2, gl.FLOAT, false, 0, 0)

  // Get uniform locations
  timeLocation = gl.getUniformLocation(program, 'u_time')
  resolutionLocation = gl.getUniformLocation(program, 'u_resolution')
  pointerLocation = gl.getUniformLocation(program, 'u_pointer')
  pointerStrengthLocation = gl.getUniformLocation(program, 'u_pointerStrength')

  return true
}

function resizeCanvas() {
  if (!canvasRef.value || !gl) return

  const isMobile = window.innerWidth < 768
  const dpr = Math.min(window.devicePixelRatio || 1, isMobile ? 1 : 1.5)
  
  const width = window.innerWidth
  const height = window.innerHeight

  canvasRef.value.width = width * dpr
  canvasRef.value.height = height * dpr
  gl.viewport(0, 0, canvasRef.value.width, canvasRef.value.height)

  gl.uniform2f(resolutionLocation, canvasRef.value.width, canvasRef.value.height)
}

function render(time) {
  if (isContextLost || document.hidden || reducedMotion || fallbackMode.value) {
    animationFrameId = requestAnimationFrame(render)
    return
  }

  const delta = time - lastRenderTime
  if (delta < FRAME_MIN_TIME) {
    animationFrameId = requestAnimationFrame(render)
    return
  }

  lastRenderTime = time - (delta % FRAME_MIN_TIME)

  // Smoothly interpolate pointer position (lerp)
  currentPointer.x += (targetPointer.x - currentPointer.x) * 0.05
  currentPointer.y += (targetPointer.y - currentPointer.y) * 0.05

  const timeSecs = (Date.now() - startTime) / 1000

  gl.uniform1f(timeLocation, timeSecs)
  // Invert Y axis for WebGL
  gl.uniform2f(pointerLocation, currentPointer.x, 1.0 - currentPointer.y)
  
  // Disable pointer effect on mobile
  const isMobile = window.innerWidth < 768
  gl.uniform1f(pointerStrengthLocation, isMobile ? 0.0 : 1.0)

  gl.drawArrays(gl.TRIANGLES, 0, 6)

  animationFrameId = requestAnimationFrame(render)
}

// Event Listeners
function handlePointerMove(e) {
  targetPointer.x = e.clientX / window.innerWidth
  targetPointer.y = e.clientY / window.innerHeight
}

function handleContextLost(e) {
  e.preventDefault()
  isContextLost = true
  fallbackMode.value = true
}

function handleContextRestored() {
  isContextLost = false
  if (initWebGL()) {
    fallbackMode.value = false
    resizeCanvas()
  }
}

let resizeTimeout
function handleResize() {
  clearTimeout(resizeTimeout)
  resizeTimeout = setTimeout(resizeCanvas, 200)
}

onMounted(() => {
  // Check reduced motion and low-power modes
  reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches
  const isLowPower = navigator.connection?.saveData === true || (navigator.deviceMemory && navigator.deviceMemory <= 4)
  
  if (reducedMotion || isLowPower) {
    fallbackMode.value = true
    return
  }

  if (initWebGL()) {
    resizeCanvas()
    window.addEventListener('resize', handleResize)
    window.addEventListener('pointermove', handlePointerMove, { passive: true })
    canvasRef.value.addEventListener('webglcontextlost', handleContextLost, false)
    canvasRef.value.addEventListener('webglcontextrestored', handleContextRestored, false)
    
    // Start render loop
    animationFrameId = requestAnimationFrame(render)
  } else {
    fallbackMode.value = true
  }
})

onBeforeUnmount(() => {
  if (animationFrameId) {
    cancelAnimationFrame(animationFrameId)
  }
  clearTimeout(resizeTimeout)
  
  window.removeEventListener('resize', handleResize)
  window.removeEventListener('pointermove', handlePointerMove)
  
  if (canvasRef.value) {
    canvasRef.value.removeEventListener('webglcontextlost', handleContextLost)
    canvasRef.value.removeEventListener('webglcontextrestored', handleContextRestored)
  }

  if (gl && program) {
    gl.deleteProgram(program)
    gl.getExtension('WEBGL_lose_context')?.loseContext()
  }
  
  gl = null
  program = null
})
</script>

<template>
  <canvas
    v-show="!fallbackMode"
    ref="canvasRef"
    class="portal-shader"
    aria-hidden="true"
  />
</template>

<style scoped>
.portal-shader {
  position: fixed;
  inset: 0;
  width: 100vw;
  height: 100vh;
  z-index: 0; /* Below grid noise (z-index:1) and content (z-index:10) */
  pointer-events: none;
  opacity: 0.12; /* Subtle visibility */
  transition: opacity 1s ease-in-out;
}
</style>
