<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const canvasRef = ref(null)
const isSupported = ref(true)

const VERT_SRC = `
attribute vec2 a_position;
varying vec2 v_texCoord;
void main() {
  gl_Position = vec4(a_position, 0.0, 1.0);
  v_texCoord = a_position * 0.5 + 0.5;
}`

const FRAG_SRC = `
precision highp float;
varying vec2 v_texCoord;
uniform float u_time;
uniform vec2 u_resolution;

vec3 permute(vec3 x) { return mod(((x*34.0)+1.0)*x, 289.0); }

float snoise(vec2 v){
  const vec4 C = vec4(0.211324865405187, 0.366025403784439,
           -0.577350269189626, 0.024390243902439);
  vec2 i  = floor(v + dot(v, C.yy) );
  vec2 x0 = v -   i + dot(i, C.xx);
  vec2 i1;
  i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
  vec4 x12 = x0.xyxy + C.xxzz;
  x12.xy -= i1;
  i = mod(i, 289.0);
  vec3 p = permute( permute( i.y + vec3(0.0, i1.y, 1.0 ))
    + i.x + vec3(0.0, i1.x, 1.0 ));
  vec3 m = max(0.5 - vec3(dot(x0,x0), dot(x12.xy,x12.xy),
    dot(x12.zw,x12.zw)), 0.0);
  m = m*m ;
  m = m*m ;
  vec3 x = 2.0 * fract(p * C.www) - 1.0;
  vec3 h = abs(x) - 0.5;
  vec3 a0 = x - floor(x + 0.5);
  vec3 g = a0 * vec3(x0.x,x12.xz) + h * vec3(x0.y,x12.yw);
  return 130.0 * dot(m, g);
}

void main() {
  vec2 uv = v_texCoord;
  float n = snoise(uv * 3.0 + u_time * 0.1);
  vec3 color1 = vec3(0.937, 0.965, 1.0);
  vec3 color2 = vec3(0.145, 0.388, 0.922);
  vec3 color3 = vec3(0.031, 0.569, 0.698);
  vec3 finalColor = mix(color1, color2, n * 0.5 + 0.5);
  finalColor = mix(finalColor, color3, snoise(uv * 2.0 - u_time * 0.05) * 0.3);
  gl_FragColor = vec4(finalColor, 0.15);
}`

let gl = null
let program = null
let animId = null
let startTime = 0
let paused = false
let contextLost = false
let reducedMotion = false

function createShader(gl, type, source) {
  const shader = gl.createShader(type)
  gl.shaderSource(shader, source)
  gl.compileShader(shader)
  if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
    console.warn('EduShader: shader compile error', gl.getShaderInfoLog(shader))
    gl.deleteShader(shader)
    return null
  }
  return shader
}

function init(canvas) {
  if (reducedMotion) return

  const prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)')
  if (prefersReduced.matches) {
    reducedMotion = true
    return
  }

  gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl')
  if (!gl) {
    isSupported.value = false
    return
  }

  const vs = createShader(gl, gl.VERTEX_SHADER, VERT_SRC)
  const fs = createShader(gl, gl.FRAGMENT_SHADER, FRAG_SRC)
  if (!vs || !fs) {
    isSupported.value = false
    gl = null
    return
  }

  program = gl.createProgram()
  gl.attachShader(program, vs)
  gl.attachShader(program, fs)
  gl.linkProgram(program)
  gl.deleteShader(vs)
  gl.deleteShader(fs)

  if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
    console.warn('EduShader: link error')
    gl.deleteProgram(program)
    program = null
    gl = null
    isSupported.value = false
    return
  }

  gl.useProgram(program)

  const positions = new Float32Array([
    -1, -1, 1, -1, -1, 1,
    -1, 1, 1, -1, 1, 1,
  ])
  const buf = gl.createBuffer()
  gl.bindBuffer(gl.ARRAY_BUFFER, buf)
  gl.bufferData(gl.ARRAY_BUFFER, positions, gl.STATIC_DRAW)

  const loc = gl.getAttribLocation(program, 'a_position')
  gl.enableVertexAttribArray(loc)
  gl.vertexAttribPointer(loc, 2, gl.FLOAT, false, 0, 0)

  const dpr = Math.min(window.devicePixelRatio || 1, 2)
  canvas.width = Math.round(canvas.clientWidth * dpr)
  canvas.height = Math.round(canvas.clientHeight * dpr)

  startTime = performance.now()
  animId = requestAnimationFrame(render)
  prefersReduced.addEventListener('change', onReducedMotionChange)
}

function render(time) {
  if (contextLost || paused || !gl || !program) return

  const dpr = Math.min(window.devicePixelRatio || 1, 2)
  const w = gl.canvas.clientWidth
  const h = gl.canvas.clientHeight
  const bw = Math.round(w * dpr)
  const bh = Math.round(h * dpr)
  if (gl.canvas.width !== bw || gl.canvas.height !== bh) {
    gl.canvas.width = bw
    gl.canvas.height = bh
  }

  gl.viewport(0, 0, gl.canvas.width, gl.canvas.height)
  gl.uniform1f(gl.getUniformLocation(program, 'u_time'), (time - startTime) * 0.001)
  gl.uniform2f(gl.getUniformLocation(program, 'u_resolution'), gl.canvas.width, gl.canvas.height)
  gl.drawArrays(gl.TRIANGLES, 0, 6)
  animId = requestAnimationFrame(render)
}

function onReducedMotionChange(e) {
  if (e.matches) {
    reducedMotion = true
    cleanup()
  } else {
    reducedMotion = false
    const canvas = canvasRef.value
    if (canvas) init(canvas)
  }
}

function onVisibilityChange() {
  if (document.hidden) {
    paused = true
    if (animId) { cancelAnimationFrame(animId); animId = null }
  } else {
    paused = false
    startTime = performance.now()
    if (!animId && !contextLost && !reducedMotion) {
      animId = requestAnimationFrame(render)
    }
  }
}

function onContextLost(e) {
  e.preventDefault()
  contextLost = true
  if (animId) { cancelAnimationFrame(animId); animId = null }
}

function onContextRestored() {
  contextLost = false
  const canvas = canvasRef.value
  if (canvas && !reducedMotion) init(canvas)
}

function cleanup() {
  if (animId) { cancelAnimationFrame(animId); animId = null }
  if (program) { gl?.deleteProgram(program); program = null }
  gl = null
}

onMounted(() => {
  const canvas = canvasRef.value
  if (!canvas) return
  init(canvas)
  document.addEventListener('visibilitychange', onVisibilityChange)
  canvas.addEventListener('webglcontextlost', onContextLost)
  canvas.addEventListener('webglcontextrestored', onContextRestored)
})

onUnmounted(() => {
  window.matchMedia('(prefers-reduced-motion: reduce)').removeEventListener('change', onReducedMotionChange)
  document.removeEventListener('visibilitychange', onVisibilityChange)
  cleanup()
})
</script>

<template>
  <canvas
    ref="canvasRef"
    v-show="isSupported"
    class="fixed inset-0 w-full h-full pointer-events-none"
    style="z-index: 0"
    aria-hidden="true"
  />
</template>
