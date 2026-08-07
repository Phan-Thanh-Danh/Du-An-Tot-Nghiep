<template>
  <div class="space-y-5 pb-10">

    <!-- Loading State -->
    <div v-if="loading" class="p-4">
      <SkeletonDashboard :cards="4" :rows="3" />
    </div>
    <!-- Error State -->
    <div v-else-if="error" class="flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3">
        <AlertCircle :size="32" class="text-(--color-danger-text)" />
        <p class="text-sm text-(--color-danger-text) font-medium">{{ error }}</p>
        <button @click="loadData()" class="px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors">Thử lại</button>
      </div>
    </div>
    <template v-else>

    <!-- ── Status Banner ── -->
    <div class="rounded-2xl bg-gradient-to-r from-(--lg-primary)/10 via-(--lg-accent)/10 to-(--lg-cyan)/10 border border-(--lg-primary)/20 px-5 py-3 flex items-center gap-3 banner-enter">
      <div class="flex h-8 w-8 items-center justify-center rounded-xl bg-(--lg-primary)/15">
        <Bell :size="16" class="text-(--lg-primary)" />
      </div>
      <p class="text-xs font-semibold text-body flex-1">
        <span class="text-(--lg-primary) font-bold">Thông báo:</span> Hệ thống đang vận hành trực tiếp với cơ sở dữ liệu học vụ SQL Server.
      </p>
      <span class="text-[10px] font-bold text-muted">Trực tuyến</span>
    </div>

    <!-- ── Page Header ── -->
    <div class="flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
      <div>
        <p class="text-[10px] font-bold text-muted uppercase tracking-widest">Trang chủ &gt; Dashboard chiến lược</p>
        <h1 class="text-xl md:text-2xl font-bold text-heading mt-1 tracking-tight">Dashboard chiến lược</h1>
        <p class="text-sm text-muted mt-1">Tổng quan hiệu suất đào tạo và hoạt động hệ thống.</p>
      </div>
    </div>

    <!-- ── Macro KPI Cards with Mini Charts ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">

      <!-- KPI 1: Giáo viên -->
      <div class="group relative overflow-hidden rounded-2xl border border-card surface-card p-4 shadow-sm transition-all hover:shadow-md kpi-card-enter" style="animation-delay: 0.05s">
        <div class="flex items-center justify-between">
          <div class="flex h-10 w-10 items-center justify-center rounded-2xl bg-(--color-info-bg) text-(--color-info-text) transition-transform group-hover:scale-110">
            <User :size="24" stroke-width="2.2" />
          </div>
          <div class="flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold bg-(--color-success-bg) text-(--color-success-text)">
            {{ teacherTrendLabel }} <ArrowUpRight v-if="teacherTrendPositive" :size="12" />
          </div>
        </div>
        <div class="mt-3">
          <p class="text-sm font-medium text-muted">Tổng giáo viên</p>
          <p class="mt-0.5 text-2xl font-bold text-heading">{{ apiData?.totalTeachers ?? 0 }}</p>
        </div>
        <div class="mt-3 relative h-14">
          <svg viewBox="0 0 200 60" class="w-full h-full" preserveAspectRatio="none">
            <defs>
              <linearGradient id="kpi1-grad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stop-color="var(--lg-primary)" stop-opacity="0.25"/>
                <stop offset="100%" stop-color="var(--lg-primary)" stop-opacity="0"/>
              </linearGradient>
            </defs>
            <path :d="kpi1AreaPath" fill="url(#kpi1-grad)" class="transition-all duration-500" />
            <path :d="kpi1MainPath" fill="none" stroke="var(--lg-primary)" stroke-width="2" stroke-linecap="round" class="mini-chart-line transition-all duration-500" />
            <path :d="kpi1SecondaryPath" fill="none" stroke="var(--color-success-text)" stroke-width="1.5" stroke-dasharray="4 3" stroke-linecap="round" class="mini-chart-line transition-all duration-500" />
            <circle v-for="(pt, idx) in kpi1MainPoints" :key="'k1m-'+idx"
              :cx="pt.x" :cy="pt.y" r="8" fill="transparent" class="cursor-pointer"
              @mouseenter="hoveredKpi1 = idx" @mouseleave="hoveredKpi1 = -1" />
            <circle v-if="hoveredKpi1 >= 0"
              :cx="kpi1MainPoints[hoveredKpi1].x" :cy="kpi1MainPoints[hoveredKpi1].y"
              r="4" fill="var(--lg-primary)" stroke="white" stroke-width="1.5" class="kpi-dot-pop" />
          </svg>
          <div class="flex items-center gap-3 mt-1">
            <span class="flex items-center gap-1 text-[9px] font-bold text-muted"><span class="h-1.5 w-3 rounded-full bg-(--lg-primary)"></span>Dạy chính</span>
            <span class="flex items-center gap-1 text-[9px] font-bold text-muted"><span class="h-1.5 w-3 rounded-full bg-(--color-success-text) opacity-60"></span>Thỉnh giảng</span>
          </div>
          <div v-if="hoveredKpi1 >= 0" class="absolute -top-1 left-1/2 -translate-x-1/2 glowing-tooltip text-white text-[10px] font-bold rounded-lg px-2 py-1 pointer-events-none z-10 whitespace-nowrap">
            {{ miniChartDays[hoveredKpi1] }}: {{ kpi1Values.dayChinh[hoveredKpi1] }} chính · {{ kpi1Values.dayThinhGiang[hoveredKpi1] }} thỉnh giảng
          </div>
        </div>
      </div>

      <!-- KPI 2: Sinh viên -->
      <div class="group relative overflow-hidden rounded-2xl border border-card surface-card p-4 shadow-sm transition-all hover:shadow-md kpi-card-enter" style="animation-delay: 0.1s">
        <div class="flex items-center justify-between">
          <div class="flex h-10 w-10 items-center justify-center rounded-2xl bg-(--color-success-bg) text-(--color-success-text) transition-transform group-hover:scale-110">
            <GraduationCap :size="24" stroke-width="2.2" />
          </div>
          <div class="flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold bg-(--color-success-bg) text-(--color-success-text)">
            {{ studentTrendLabel }} <ArrowUpRight v-if="studentTrendPositive" :size="12" />
          </div>
        </div>
        <div class="mt-3">
          <p class="text-sm font-medium text-muted">Tổng sinh viên</p>
          <p class="mt-0.5 text-2xl font-bold text-heading">{{ apiData?.totalStudents ?? 0 }}</p>
        </div>
        <div class="mt-3 relative h-14">
          <svg viewBox="0 0 200 60" class="w-full h-full" preserveAspectRatio="none">
            <defs>
              <linearGradient id="kpi2-grad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stop-color="var(--color-success-text)" stop-opacity="0.2"/>
                <stop offset="100%" stop-color="var(--color-success-text)" stop-opacity="0"/>
              </linearGradient>
            </defs>
            <path :d="kpi2AreaPath" fill="url(#kpi2-grad)" class="transition-all duration-500" />
            <path :d="kpi2Path" fill="none" stroke="var(--color-success-text)" stroke-width="2" stroke-linecap="round" class="mini-chart-line transition-all duration-500" />
            <circle v-for="(pt, idx) in kpi2Points" :key="'k2-'+idx"
              :cx="pt.x" :cy="pt.y" r="8" fill="transparent" class="cursor-pointer"
              @mouseenter="hoveredKpi2 = idx" @mouseleave="hoveredKpi2 = -1" />
            <circle v-if="hoveredKpi2 >= 0"
              :cx="kpi2Points[hoveredKpi2].x" :cy="kpi2Points[hoveredKpi2].y"
              r="4" fill="var(--color-success-text)" stroke="white" stroke-width="1.5" class="kpi-dot-pop" />
          </svg>
          <div v-if="hoveredKpi2 >= 0" class="absolute -top-1 left-1/2 -translate-x-1/2 glowing-tooltip text-white text-[10px] font-bold rounded-lg px-2 py-1 pointer-events-none z-10 whitespace-nowrap">
            {{ miniChartDays[hoveredKpi2] }}: {{ kpi2Values[hoveredKpi2] }} SV
          </div>
        </div>
      </div>

      <!-- KPI 3: Lớp học -->
      <div class="group relative overflow-hidden rounded-2xl border border-card surface-card p-4 shadow-sm transition-all hover:shadow-md kpi-card-enter" style="animation-delay: 0.15s">
        <div class="flex items-center justify-between">
          <div class="flex h-10 w-10 items-center justify-center rounded-2xl bg-(--color-warning-bg) text-(--color-warning-text) transition-transform group-hover:scale-110">
            <BarChart2 :size="24" stroke-width="2.2" />
          </div>
          <div class="flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold bg-(--color-success-bg) text-(--color-success-text)">
            {{ classTrendLabel }}
          </div>
        </div>
        <div class="mt-3">
          <p class="text-sm font-medium text-muted">Tổng lớp học</p>
          <p class="mt-0.5 text-2xl font-bold text-heading">{{ apiData?.totalClasses ?? 0 }}</p>
        </div>
        <div class="mt-3 relative h-14">
          <svg viewBox="0 0 200 60" class="w-full h-full" preserveAspectRatio="none">
            <defs>
              <linearGradient id="kpi3-grad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stop-color="var(--color-warning-text)" stop-opacity="0.2"/>
                <stop offset="100%" stop-color="var(--color-warning-text)" stop-opacity="0"/>
              </linearGradient>
            </defs>
            <path :d="kpi3AreaPath" fill="url(#kpi3-grad)" class="transition-all duration-500" />
            <path :d="kpi3Path" fill="none" stroke="var(--color-warning-text)" stroke-width="2" stroke-linecap="round" class="mini-chart-line transition-all duration-500" />
            <circle v-for="(pt, idx) in kpi3Points" :key="'k3-'+idx"
              :cx="pt.x" :cy="pt.y" r="8" fill="transparent" class="cursor-pointer"
              @mouseenter="hoveredKpi3 = idx" @mouseleave="hoveredKpi3 = -1" />
            <circle v-if="hoveredKpi3 >= 0"
              :cx="kpi3Points[hoveredKpi3].x" :cy="kpi3Points[hoveredKpi3].y"
              r="4" fill="var(--color-warning-text)" stroke="white" stroke-width="1.5" class="kpi-dot-pop" />
          </svg>
          <div v-if="hoveredKpi3 >= 0" class="absolute -top-1 left-1/2 -translate-x-1/2 glowing-tooltip text-white text-[10px] font-bold rounded-lg px-2 py-1 pointer-events-none z-10 whitespace-nowrap">
            {{ miniChartDays[hoveredKpi3] }}: {{ kpi3Values[hoveredKpi3] }} lớp
          </div>
        </div>
      </div>

      <!-- KPI 4: TKB chờ duyệt -->
      <div class="group relative overflow-hidden rounded-2xl border border-card surface-card p-4 shadow-sm transition-all hover:shadow-md kpi-card-enter" style="animation-delay: 0.2s">
        <div class="flex items-center justify-between">
          <div :class="['flex h-10 w-10 items-center justify-center rounded-2xl transition-transform group-hover:scale-110', pendingCount > 0 ? 'bg-(--color-danger-bg) text-(--color-danger-text)' : 'bg-(--color-success-bg) text-(--color-success-text)']">
            <Clock :size="24" stroke-width="2.2" />
          </div>
          <div :class="['flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold', pendingCount > 0 ? 'bg-(--color-danger-bg) text-(--color-danger-text)' : 'bg-(--color-success-bg) text-(--color-success-text)']">
            {{ pendingCount > 0 ? 'Cần xử lý' : 'Đã duyệt' }}
          </div>
        </div>
        <div class="mt-3">
          <p class="text-sm font-medium text-muted">TKB chờ duyệt</p>
          <p class="mt-0.5 text-2xl font-bold text-heading">{{ pendingCount }}</p>
        </div>
        <div class="mt-3 relative h-14">
          <svg viewBox="0 0 200 60" class="w-full h-full" preserveAspectRatio="none">
            <path :d="kpi4Path" fill="none" stroke="var(--color-muted)" stroke-width="1.5" stroke-linecap="round" opacity="0.6" class="mini-chart-line transition-all duration-500" />
            <g v-for="(pt, idx) in kpi4Points" :key="'k4-'+idx">
              <circle :cx="pt.x" :cy="pt.y" r="8" fill="transparent" class="cursor-pointer"
                @mouseenter="hoveredKpi4 = idx" @mouseleave="hoveredKpi4 = -1" />
              <g v-if="kpi4Values[idx] >= 1">
                <line :x1="pt.x - 3" :y1="pt.y - 3" :x2="pt.x + 3" :y2="pt.y + 3" stroke="var(--color-danger-text)" stroke-width="2" />
                <line :x1="pt.x + 3" :y1="pt.y - 3" :x2="pt.x - 3" :y2="pt.y + 3" stroke="var(--color-danger-text)" stroke-width="2" />
              </g>
            </g>
          </svg>
          <div v-if="hoveredKpi4 >= 0" class="absolute -top-1 left-1/2 -translate-x-1/2 glowing-tooltip text-white text-[10px] font-bold rounded-lg px-2 py-1 pointer-events-none z-10 whitespace-nowrap">
            {{ miniChartDays[hoveredKpi4] }}: {{ kpi4Values[hoveredKpi4] }} TKB chờ
          </div>
        </div>
      </div>
    </div>

    <!-- ══════ Main 2-Column Layout ══════ -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-5">

      <!-- ═════ Left Column (2/3) ═════ -->
      <div class="xl:col-span-2 space-y-5">

        <!-- ── Ranking Giảng viên ── -->
        <div v-if="topTeachers.length" class="rounded-2xl border border-card surface-card shadow-sm overflow-hidden section-enter" style="animation-delay: 0.08s">
          <div class="flex items-center justify-between px-5 py-4 border-b border-default">
            <div>
              <h2 class="text-base font-bold text-heading">Ranking Giảng viên</h2>
              <p class="text-[11px] text-muted mt-0.5 font-medium">Top giảng viên có điểm đánh giá cao nhất</p>
            </div>
            <router-link to="/bgh/evaluations/ranking" class="text-xs font-bold text-link hover:underline transition-colors">Tất cả xếp hạng →</router-link>
          </div>
          <div class="p-4 grid grid-cols-1 md:grid-cols-2 gap-3">
            <div v-for="(teacher, tIdx) in topTeachers" :key="teacher.id"
                 class="group flex items-center gap-4 rounded-2xl border border-default p-4 transition-all hover:border-(--border-input-focus) hover:shadow-sm teacher-card-enter"
                 :style="{ animationDelay: `${0.12 + tIdx * 0.05}s` }">
              <div class="relative">
                <div class="h-11 w-11 rounded-2xl bg-(--color-info-bg) text-(--color-info-text) flex items-center justify-center font-bold text-sm shadow-sm">{{ teacher.initials }}</div>
                <div v-if="tIdx === 0" class="absolute -top-1 -right-1 h-5 w-5 rounded-full bg-(--color-warning-bg) text-(--color-warning-text) flex items-center justify-center">
                  <Star :size="10" fill="currentColor" />
                </div>
              </div>
              <div class="flex-1 min-w-0">
                <h3 class="font-bold text-heading text-sm truncate">{{ teacher.name }}</h3>
                <p class="text-[11px] text-muted">{{ teacher.department }}</p>
              </div>
              <div class="text-right">
                <div class="flex items-center justify-end gap-1 text-sm font-bold text-heading">
                  <Star class="w-3.5 h-3.5 text-(--color-warning-text)" fill="currentColor" /> {{ teacher.rating }}
                </div>
                <p class="text-[10px] text-muted font-medium">{{ teacher.reviews }} lượt</p>
              </div>
            </div>
          </div>
        </div>

        <!-- ── Pass/Fail Time-Series Chart ── -->
        <div class="rounded-2xl border border-card surface-card shadow-sm section-enter" style="animation-delay: 0.15s">
          <!-- Header -->
          <div class="flex items-center justify-between px-5 py-4 border-b border-default rounded-t-2xl">
            <div>
              <h2 class="text-base font-bold text-heading">Tỷ lệ Pass / Fail</h2>
              <p class="text-[11px] text-muted mt-0.5 font-medium">Phân tích theo ngành, chuyên ngành, môn học và học kỳ</p>
            </div>
            <div class="flex items-center gap-3 text-[10px]">
              <span class="flex items-center gap-1.5"><span class="h-2.5 w-2.5 rounded-full bg-[#10B981]"></span><span class="font-bold text-muted">Pass</span></span>
              <span class="flex items-center gap-1.5"><span class="h-2.5 w-2.5 rounded-full bg-[#EF4444]"></span><span class="font-bold text-muted">Fail</span></span>
            </div>
          </div>

          <!-- 4 Cascade Combo Boxes -->
          <div class="px-5 pt-4 pb-2 grid grid-cols-2 lg:grid-cols-4 gap-3">
            <LmsSelect
              v-model="selectedMajor"
              :options="majorOptions"
              placeholder="Ngành đào tạo"
            />
            <LmsSelect
              v-model="selectedSpec"
              :options="specOptions"
              placeholder="Chuyên ngành"
            />
            <LmsSelect
              v-model="selectedSubjectFilter"
              :options="subjectFilterOptions"
              placeholder="Môn học"
            />
            <LmsSelect
              v-model="selectedSemesterFilter"
              :options="semesterFilterOptions"
              placeholder="Học kỳ"
            />
          </div>

          <!-- Chart (full width, minimal padding) -->
          <div class="px-1 pb-3 pt-1">
            <div class="relative w-full chart-container-enter" style="padding-bottom: 26%;">
              <svg class="absolute inset-0 w-full h-full" viewBox="0 0 1000 270" preserveAspectRatio="xMidYMid meet">
                <!-- Area Gradients -->
                <defs>
                  <linearGradient id="pass-area-grad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stop-color="#10B981" stop-opacity="0.16" />
                    <stop offset="100%" stop-color="#10B981" stop-opacity="0.0" />
                  </linearGradient>
                  <linearGradient id="fail-area-grad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stop-color="#EF4444" stop-opacity="0.10" />
                    <stop offset="100%" stop-color="#EF4444" stop-opacity="0.0" />
                  </linearGradient>
                </defs>

                <!-- Static Grid Lines (Soft appearance 0.15) -->
                <line v-for="i in 5" :key="'gl-'+i" x1="45" :y1="36 + (i-1)*50" x2="975" :y2="36 + (i-1)*50"
                  stroke="var(--border-default)" stroke-width="1" class="chart-grid-line" />
                <!-- Y-axis labels -->
                <text x="38" y="40" class="text-[10px] font-bold fill-current text-muted chart-grid-line" text-anchor="end">100%</text>
                <text x="38" y="90" class="text-[10px] font-bold fill-current text-muted chart-grid-line" text-anchor="end">80%</text>
                <text x="38" y="140" class="text-[10px] font-bold fill-current text-muted chart-grid-line" text-anchor="end">60%</text>
                <text x="38" y="190" class="text-[10px] font-bold fill-current text-muted chart-grid-line" text-anchor="end">40%</text>
                <text x="38" y="240" class="text-[10px] font-bold fill-current text-muted chart-grid-line" text-anchor="end">20%</text>

                <!-- Crosshair Line on Hover -->
                <line
                  v-if="hoveredChartPoint"
                  :x1="hoveredChartPoint.x" y1="36"
                  :x2="hoveredChartPoint.x" y2="236"
                  stroke="var(--border-default)" stroke-width="1" stroke-dasharray="3 3"
                  class="chart-crosshair"
                />

                <!-- SVG Clean Group for Lines, Areas and Nodes -->
                <g>
                  <!-- X-axis labels -->
                  <text v-for="(label, idx) in chartLabels" :key="'xl-'+idx"
                    :x="chartX(idx)" y="262"
                    class="text-[10px] font-bold fill-current text-muted transition-all duration-300" text-anchor="middle">
                    {{ label }}
                  </text>

                  <!-- Pass Area Fill -->
                  <path :d="passAreaPath" fill="url(#pass-area-grad)"
                    :class="isInitialPhase ? 'chart-area-initial-pass' : 'chart-area-morph'" />

                  <!-- Fail Area Fill -->
                  <path :d="failAreaPath" fill="url(#fail-area-grad)"
                    :class="isInitialPhase ? 'chart-area-initial-fail' : 'chart-area-morph'" />

                  <!-- Pass Line (Clear Polyline Green) -->
                  <path :d="passLinePath" fill="none" stroke="#10B981" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"
                    :class="isInitialPhase ? 'chart-line-draw-pass' : ''" />

                  <!-- Fail Line (Clear Polyline Red) -->
                  <path :d="failLinePath" fill="none" stroke="#EF4444" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"
                    :class="isInitialPhase ? 'chart-line-draw-fail' : ''" />

                  <!-- Pass dots (Sequential Pop-in on Initial / Smooth Morph on Filter) -->
                  <g v-for="(pt, idx) in renderedPassPoints" :key="'pd-'+idx"
                     class="cursor-pointer"
                     :class="isInitialPhase ? 'chart-dot-initial-pass' : ''"
                     :style="getPassDotStyle(pt, idx)">
                    <circle cx="0" cy="0" r="14" fill="transparent"
                      @mouseenter="hoveredChartPoint = { type: 'Pass', idx, val: pt.val, x: pt.x, y: pt.y }"
                      @mouseleave="hoveredChartPoint = null" />
                    <circle cx="0" cy="0" r="4.5" fill="#10B981" stroke="white" stroke-width="1.5" />
                  </g>

                  <!-- Fail dots (Sequential Pop-in on Initial / Smooth Morph on Filter) -->
                  <g v-for="(pt, idx) in renderedFailPoints" :key="'fd-'+idx"
                     class="cursor-pointer"
                     :class="isInitialPhase ? 'chart-dot-initial-fail' : ''"
                     :style="getFailDotStyle(pt, idx)">
                    <circle cx="0" cy="0" r="14" fill="transparent"
                      @mouseenter="hoveredChartPoint = { type: 'Fail', idx, val: pt.val, x: pt.x, y: pt.y }"
                      @mouseleave="hoveredChartPoint = null" />
                    <circle cx="0" cy="0" r="4.5" fill="#EF4444" stroke="white" stroke-width="1.5" />
                  </g>
                </g>
              </svg>

              <!-- Chart Tooltip (Never clipped, high z-index & smart boundary alignment) -->
              <div
                v-if="hoveredChartPoint"
                class="absolute glowing-tooltip text-white text-[10px] font-bold rounded-lg px-2.5 py-1.5 pointer-events-none whitespace-nowrap chart-tooltip-anim"
                :style="tooltipStyle"
              >
                {{ chartLabels[hoveredChartPoint.idx] }} · {{ hoveredChartPoint.type }}: {{ hoveredChartPoint.val }}%
                <div
                  class="absolute top-full -translate-x-1/2 w-0 h-0 border-l-4 border-l-transparent border-r-4 border-r-transparent border-t-4 border-t-slate-900 dark:border-t-slate-800"
                  :style="tooltipArrowStyle"
                ></div>
              </div>
            </div>
          </div>
        </div>

      </div>

      <!-- ═════ Right Column (1/3) ═════ -->
      <div class="space-y-5">

        <!-- ── TKB Pending ── -->
        <div class="rounded-2xl border border-card surface-card shadow-sm overflow-hidden section-enter" style="animation-delay: 0.15s">
          <div class="flex items-center justify-between px-5 py-4 border-b border-default">
            <h3 class="text-base font-bold text-heading">TKB Chờ Duyệt</h3>
            <span v-if="pendingCount > 0" class="rounded-full bg-(--color-info-bg) px-2.5 py-0.5 text-[10px] font-bold text-(--color-info-text)">{{ pendingCount }} Mới</span>
          </div>
          <div class="p-4 space-y-3">
            <div v-for="item in pendingScheduleItems" :key="item.id"
                 class="p-3 rounded-xl border border-default surface-solid transition-all hover:bg-(--surface-input) cursor-pointer group">
              <div class="flex justify-between items-start">
                <p class="text-xs font-bold text-heading leading-tight">{{ item.title }}</p>
                <span class="text-[9px] font-bold text-(--lg-primary) bg-(--lg-primary)/10 px-1.5 py-0.5 rounded">{{ item.badge }}</span>
              </div>
              <p class="mt-1 text-[10px] text-muted">{{ item.description }}</p>
              <router-link to="/bgh/schedule/pending" class="mt-2 block text-center text-[10px] font-bold text-link group-hover:underline transition-colors">Xem ngay →</router-link>
            </div>
            <div v-if="!pendingScheduleItems.length" class="py-6 text-center">
              <p class="text-xs text-muted font-medium">Không có TKB nào chờ duyệt</p>
            </div>
          </div>
        </div>

        <!-- ── AI Risk Alerts ── -->
        <div class="rounded-2xl border border-(--color-danger-text)/20 bg-(--color-danger-bg)/50 overflow-hidden section-enter" style="animation-delay: 0.2s">
          <div class="flex items-center gap-2 px-5 py-4 border-b border-(--color-danger-text)/15">
            <AlertCircle :size="16" class="text-(--color-danger-text)" />
            <h3 class="text-base font-bold text-heading">Cảnh báo rủi ro</h3>
          </div>
          <div class="p-4">
            <p class="text-xs text-body font-medium">AI phát hiện <span class="font-bold text-(--color-danger-text)">{{ riskStudents.length }}</span> sinh viên có rủi ro rớt môn cao.</p>
            <div class="mt-3 space-y-2.5">
              <div v-for="sv in riskStudents" :key="sv.id"
                   class="flex items-center justify-between py-2 border-b border-(--color-danger-text)/10 last:border-0">
                <div>
                  <p class="text-xs font-bold text-heading">{{ sv.name }}</p>
                  <p class="text-[9px] text-muted">{{ sv.class }}</p>
                </div>
                <span class="text-[9px] font-bold bg-(--surface-card) text-(--color-danger-text) px-2 py-0.5 rounded-full border border-(--color-danger-text)/20">{{ sv.reason }}</span>
              </div>
            </div>
            <router-link to="/bgh/academic/at-risk" class="mt-4 block text-center text-[10px] font-bold text-(--color-danger-text) hover:underline transition-colors">Xem toàn bộ báo cáo rủi ro →</router-link>
          </div>
        </div>

        <!-- ── Announcements ── -->
        <div class="rounded-2xl border border-card surface-card shadow-sm overflow-hidden section-enter" style="animation-delay: 0.25s">
          <div class="flex items-center justify-between px-5 py-4 border-b border-default">
            <h3 class="text-base font-bold text-heading">Thông báo</h3>
            <Bell :size="16" class="text-muted" />
          </div>
          <div class="p-4 space-y-3">
            <div v-for="(notif, nIdx) in announcements" :key="nIdx" class="flex gap-3 group">
              <div :class="['h-8 w-8 rounded-xl flex items-center justify-center shrink-0', notif.bgColor, notif.iconColor]">
                <component :is="notif.icon" :size="14" />
              </div>
              <div class="flex-1 min-w-0">
                <p class="text-xs font-bold text-heading truncate">{{ notif.title }}</p>
                <p class="text-[10px] text-muted mt-0.5 leading-relaxed">{{ notif.description }}</p>
                <p class="text-[9px] text-placeholder mt-1 font-medium">{{ notif.time }}</p>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>

    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import SkeletonDashboard from '@/components/common/skeleton/SkeletonDashboard.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import {
  AlertCircle, BarChart2, Star, GraduationCap, Clock, User,
  ArrowUpRight, Bell, ShieldCheck, FileText
} from 'lucide-vue-next'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

defineOptions({ name: 'BghDashboard' })

const loading = ref(false)
const error = ref(null)
const apiData = ref(null)

// Flag quản lý animation lần đầu vẽ từ trái qua phải
const isInitialDrawFinished = ref(false)

// ══════════════════════════════════════
// ── Mini Chart Helpers
// ══════════════════════════════════════
const miniChartDays = computed(() => {
  const today = new Date()
  return Array.from({ length: 7 }, (_, index) => {
    const day = new Date(today)
    day.setDate(today.getDate() - 6 + index)
    return day.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' })
  })
})

function flatSeries(value) {
  return Array.from({ length: miniChartDays.value.length }, () => Number(value) || 0)
}

const kpi1Values = computed(() => ({
  dayChinh: flatSeries(apiData.value?.totalTeachers),
  dayThinhGiang: flatSeries(apiData.value?.totalTeachers),
}))
const kpi2Values = computed(() => flatSeries(apiData.value?.totalStudents))
const kpi3Values = computed(() => flatSeries(apiData.value?.totalClasses))
const kpi4Values = computed(() => flatSeries(apiData.value?.pendingSchedules))
const teacherTrendPositive = computed(() => (apiData.value?.totalTeachers ?? 0) > 0)
const studentTrendPositive = computed(() => (apiData.value?.totalStudents ?? 0) > 0)
const teacherTrendLabel = computed(() => `${apiData.value?.totalTeachers ?? 0}`)
const studentTrendLabel = computed(() => `${apiData.value?.totalStudents ?? 0}`)
const classTrendLabel = computed(() => `${apiData.value?.totalClasses ?? 0}`)

const hoveredKpi1 = ref(-1)
const hoveredKpi2 = ref(-1)
const hoveredKpi3 = ref(-1)
const hoveredKpi4 = ref(-1)

function miniPts(values, maxVal) {
  const mx = maxVal || Math.max(...values)
  return values.map((v, i) => ({
    x: 5 + (i / (values.length - 1)) * 190,
    y: 55 - (v / (mx || 1)) * 45,
    val: v
  }))
}
function smoothLine(pts) {
  if (pts.length < 2) return ''
  let d = `M ${pts[0].x} ${pts[0].y}`
  for (let i = 1; i < pts.length; i++) {
    const cpx = (pts[i - 1].x + pts[i].x) / 2
    d += ` C ${cpx} ${pts[i - 1].y}, ${cpx} ${pts[i].y}, ${pts[i].x} ${pts[i].y}`
  }
  return d
}
function areaFill(pts) {
  const l = smoothLine(pts)
  return l ? l + ` L ${pts[pts.length - 1].x} 60 L ${pts[0].x} 60 Z` : ''
}

const kpi1MainPoints = computed(() => miniPts(kpi1Values.value.dayChinh, Math.max(...kpi1Values.value.dayChinh, 1)))
const kpi1SecondaryPoints = computed(() => miniPts(kpi1Values.value.dayThinhGiang, Math.max(...kpi1Values.value.dayThinhGiang, 1)))
const kpi1MainPath = computed(() => smoothLine(kpi1MainPoints.value))
const kpi1SecondaryPath = computed(() => smoothLine(kpi1SecondaryPoints.value))
const kpi1AreaPath = computed(() => areaFill(kpi1MainPoints.value))
const kpi2Points = computed(() => miniPts(kpi2Values.value, Math.max(...kpi2Values.value, 1)))
const kpi2Path = computed(() => smoothLine(kpi2Points.value))
const kpi2AreaPath = computed(() => areaFill(kpi2Points.value))
const kpi3Points = computed(() => miniPts(kpi3Values.value, Math.max(...kpi3Values.value, 1)))
const kpi3Path = computed(() => smoothLine(kpi3Points.value))
const kpi3AreaPath = computed(() => areaFill(kpi3Points.value))
const kpi4Points = computed(() => miniPts(kpi4Values.value, Math.max(...kpi4Values.value, 1)))
const kpi4Path = computed(() => smoothLine(kpi4Points.value))

// ══════════════════════════════════════
// ── Cascade Filter State (Real BE Data)
// ══════════════════════════════════════
const selectedMajor = ref('')
const selectedSpec = ref('')
const selectedSubjectFilter = ref('')
const selectedSemesterFilter = ref('')
const passFailFilters = ref({ majors: [], specializations: [], programSubjects: [], semesters: [] })
const passFailData = ref(null)
const passFailRequestSeq = ref(0)

// ── Computed Options ──
const majorOptions = computed(() => [
  { value: '', label: 'Tất cả ngành' },
  ...passFailFilters.value.majors.map(m => ({ value: String(m.id), label: m.label }))
])

const specOptions = computed(() => [
  { value: '', label: 'Tất cả chuyên ngành' },
  ...passFailFilters.value.specializations.map(s => ({ value: String(s.id), label: s.label }))
])

const subjectFilterOptions = computed(() => [
  { value: '', label: 'Tất cả môn học' },
  ...passFailFilters.value.programSubjects.map(s => ({
    value: String(s.id),
    label: s.subjectCode ? `${s.subjectCode} · ${s.label}` : s.label
  }))
])

const semesterFilterOptions = computed(() => [
  { value: '', label: 'Tất cả học kỳ' },
  ...passFailFilters.value.semesters.map(s => ({
    value: String(s.id),
    label: s.academicYear ? `${s.label} (${s.academicYear})` : s.label
  }))
])

function passFailFilterParams() {
  return {
    majorId: selectedMajor.value,
    specializationId: selectedSpec.value,
    programSubjectId: selectedSubjectFilter.value,
  }
}

async function loadPassFailFilters() {
  try {
    const response = await bghApi.getPassFailFilterOptions(passFailFilterParams())
    applyPassFailFilters(response)
  } catch (e) {
    console.error('Lỗi tải bộ lọc Pass/Fail:', e)
  }
}

function applyPassFailFilters(response) {
  const data = unwrapApiData(response) || {}
  passFailFilters.value = {
    majors: Array.isArray(data.majors) ? data.majors : [],
    specializations: Array.isArray(data.specializations) ? data.specializations : [],
    programSubjects: Array.isArray(data.programSubjects) ? data.programSubjects : [],
    semesters: Array.isArray(data.semesters) ? data.semesters : [],
  }
}

async function loadPassFailData() {
  const seq = ++passFailRequestSeq.value
  try {
    const response = await bghApi.getPassFailRates({
      ...passFailFilterParams(),
      semesterId: selectedSemesterFilter.value,
    })
    if (seq === passFailRequestSeq.value) {
      passFailData.value = unwrapApiData(response)
    }
  } catch (e) {
    console.error('Lỗi tải dữ liệu Pass/Fail:', e)
  }
}

// ── Cascade watchers ──
watch(selectedMajor, async () => {
  selectedSpec.value = ''
  selectedSubjectFilter.value = ''
  selectedSemesterFilter.value = ''
  await Promise.all([loadPassFailFilters(), loadPassFailData()])
})
watch(selectedSpec, async () => {
  selectedSubjectFilter.value = ''
  selectedSemesterFilter.value = ''
  await Promise.all([loadPassFailFilters(), loadPassFailData()])
})
watch(selectedSubjectFilter, async () => {
  selectedSemesterFilter.value = ''
  await Promise.all([loadPassFailFilters(), loadPassFailData()])
})
watch(selectedSemesterFilter, async () => {
  await loadPassFailData()
})

// ══════════════════════════════════════
// ── Large Chart Computed Data (Real BE)
// ══════════════════════════════════════
const EMPTY_CHART_LABELS = ['Chưa có dữ liệu', '', '', '']
const EMPTY_ZERO_SERIES = [0, 0, 0, 0]

const currentChartData = computed(() => {
  const data = passFailData.value
  if (!data || data.totalResults === 0) return getZeroFlatChartData()

  // Trường hợp 1: Có xu hướng qua các học kỳ (semesterTrend)
  if (Array.isArray(data.semesterTrend) && data.semesterTrend.length > 0) {
    const items = data.semesterTrend
    if (items.length === 1) {
      const label = items[0].academicYear && !items[0].semesterName?.includes(String(items[0].academicYear))
        ? `${items[0].semesterName} ${items[0].academicYear}`
        : (items[0].semesterName || '—')
      const p = Number(items[0].passRate) || 0
      const f = Number(items[0].failRate) || 0
      if ((items[0].total ?? 0) === 0 || (p === 0 && f === 0)) {
        return getZeroFlatChartData()
      }
      return {
        labels: [label, ''],
        pass: [p, p],
        fail: [f, f],
      }
    }
    const hasAnyData = items.some(item => (item.total ?? 0) > 0 || (Number(item.passRate) || 0) > 0 || (Number(item.failRate) || 0) > 0)
    if (!hasAnyData) return getZeroFlatChartData()

    return {
      labels: items.map(item => {
        if (!item.semesterName) return '—'
        if (item.academicYear && !item.semesterName.includes(String(item.academicYear))) {
          return `${item.semesterName} ${item.academicYear}`
        }
        return item.semesterName
      }),
      pass: items.map(item => Number(item.passRate) || 0),
      fail: items.map(item => Number(item.failRate) || 0),
    }
  }

  // Trường hợp 2: Có danh sách môn học (courseStats)
  if (Array.isArray(data.courseStats) && data.courseStats.length > 0) {
    const topCourses = data.courseStats.slice(0, 6)
    if (topCourses.length === 1) {
      const name = topCourses[0].subjectName || topCourses[0].tenMonHoc || '—'
      const label = name.length > 12 ? name.slice(0, 12) + '…' : name
      const total = topCourses[0].total || 0
      if (total === 0) return getZeroFlatChartData()
      const p = Math.round((topCourses[0].pass / total) * 100)
      const f = Math.round((topCourses[0].fail / total) * 100)
      if (p === 0 && f === 0) return getZeroFlatChartData()
      return {
        labels: [label, ''],
        pass: [p, p],
        fail: [f, f],
      }
    }
    const hasAnyData = topCourses.some(item => (item.total ?? 0) > 0)
    if (!hasAnyData) return getZeroFlatChartData()

    return {
      labels: topCourses.map(item => {
        const name = item.subjectName || item.tenMonHoc || '—'
        return name.length > 12 ? name.slice(0, 12) + '…' : name
      }),
      pass: topCourses.map(item => {
        const total = item.total || 1
        return Math.round((item.pass / total) * 100)
      }),
      fail: topCourses.map(item => {
        const total = item.total || 1
        return Math.round((item.fail / total) * 100)
      }),
    }
  }

  return getZeroFlatChartData()
})

function getZeroFlatChartData() {
  return {
    labels: EMPTY_CHART_LABELS,
    pass: EMPTY_ZERO_SERIES,
    fail: EMPTY_ZERO_SERIES,
  }
}

const chartLabels = computed(() => currentChartData.value.labels)
const chartPointCount = computed(() => chartLabels.value.length)
const hoveredChartPoint = ref(null)

// ── SVG Coordinate Helpers ──
const CHART_X_START = 55
const CHART_X_END = 975
const CHART_Y_TOP = 36   // 100%
const CHART_Y_BOT = 236  // 0%

function chartX(idx) {
  const n = chartPointCount.value
  if (n <= 1) return (CHART_X_START + CHART_X_END) / 2
  return CHART_X_START + (idx / (n - 1)) * (CHART_X_END - CHART_X_START)
}
function chartY(pct) {
  const val = Number(pct)
  if (isNaN(val) || val <= 0) return CHART_Y_BOT // Strictly 236 (0% baseline at bottom)
  if (val >= 100) return CHART_Y_TOP             // Strictly 36 (100% at top)
  return CHART_Y_BOT - (val / 100) * (CHART_Y_BOT - CHART_Y_TOP)
}

// ══════════════════════════════════════
// ── Motion Animation Engine (Senior Spec)
// ══════════════════════════════════════
const isInitialPhase = ref(true)

const targetPassPoints = computed(() => currentChartData.value.pass.map((v, i) => ({ x: chartX(i), y: chartY(v), val: v })))
const targetFailPoints = computed(() => currentChartData.value.fail.map((v, i) => ({ x: chartX(i), y: chartY(v), val: v })))

const renderedPassPoints = ref([])
const renderedFailPoints = ref([])

let morphFrameId = null
let initialPhaseTimer = null
let initialDrawTimer = null
let morphStartTime = 0
let startPassPoints = []
let startFailPoints = []
const MORPH_DURATION = 580 // ms (500–650ms range easeInOutCubic)

function easeInOutCubic(t) {
  return t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2
}

// Nội suy lấy điểm xuất phát từ mảng rendered cũ khớp với trục X mới
function resamplePoints(oldPoints, targetPoints) {
  if (!oldPoints || oldPoints.length === 0) {
    return targetPoints.map(p => ({ x: p.x, y: CHART_Y_BOT, val: 0 }))
  }
  if (oldPoints.length === targetPoints.length) {
    return oldPoints.map(p => ({ ...p }))
  }

  const firstOld = oldPoints[0]
  const lastOld = oldPoints[oldPoints.length - 1]

  return targetPoints.map(targetP => {
    if (targetP.x <= firstOld.x) {
      return { x: targetP.x, y: firstOld.y, val: firstOld.val }
    }
    if (targetP.x >= lastOld.x) {
      return { x: targetP.x, y: lastOld.y, val: lastOld.val }
    }
    for (let k = 0; k < oldPoints.length - 1; k++) {
      const p1 = oldPoints[k]
      const p2 = oldPoints[k + 1]
      if (targetP.x >= p1.x && targetP.x <= p2.x) {
        const ratio = (p2.x === p1.x) ? 0 : (targetP.x - p1.x) / (p2.x - p1.x)
        const interpolatedY = p1.y + (p2.y - p1.y) * ratio
        return { x: targetP.x, y: interpolatedY, val: p1.val }
      }
    }
    return { x: targetP.x, y: firstOld.y, val: firstOld.val }
  })
}

function triggerMorph() {
  if (isInitialPhase.value) return

  // Fast switching: cancel previous animation immediately (never queue!)
  if (morphFrameId) cancelAnimationFrame(morphFrameId)

  const tPass = targetPassPoints.value
  const tFail = targetFailPoints.value

  // Nội suy mượt điểm bắt đầu từ mảng cũ tại tọa độ X mới
  startPassPoints = resamplePoints(renderedPassPoints.value, tPass)
  startFailPoints = resamplePoints(renderedFailPoints.value, tFail)

  morphStartTime = performance.now()

  function step(now) {
    const elapsed = now - morphStartTime
    const rawProgress = Math.min(1, elapsed / MORPH_DURATION)
    const ease = easeInOutCubic(rawProgress)

    const currentTPass = targetPassPoints.value
    const currentTFail = targetFailPoints.value

    renderedPassPoints.value = currentTPass.map((tp, i) => {
      const sp = startPassPoints[i] || tp
      return {
        x: sp.x + (tp.x - sp.x) * ease,
        y: sp.y + (tp.y - sp.y) * ease,
        val: ease > 0.5 ? tp.val : sp.val
      }
    })

    renderedFailPoints.value = currentTFail.map((tf, i) => {
      const sf = startFailPoints[i] || tf
      return {
        x: sf.x + (tf.x - sf.x) * ease,
        y: sf.y + (tf.y - sf.y) * ease,
        val: ease > 0.5 ? tf.val : sf.val
      }
    })

    if (rawProgress < 1) {
      morphFrameId = requestAnimationFrame(step)
    }
  }

  morphFrameId = requestAnimationFrame(step)
}

watch([targetPassPoints, targetFailPoints], () => {
  if (isInitialPhase.value) {
    renderedPassPoints.value = targetPassPoints.value.map(p => ({ ...p }))
    renderedFailPoints.value = targetFailPoints.value.map(p => ({ ...p }))
  } else {
    triggerMorph()
  }
}, { immediate: true, deep: true })

function chartLinear(pts) {
  if (!pts || pts.length === 0) return ''
  let d = `M ${pts[0].x} ${pts[0].y}`
  for (let i = 1; i < pts.length; i++) {
    d += ` L ${pts[i].x} ${pts[i].y}`
  }
  return d
}

function chartArea(pts, yBaseline = 236) {
  if (!pts || pts.length === 0) return ''
  const isAllFlatAtBaseline = pts.every(p => Math.abs(p.y - yBaseline) < 1)
  if (isAllFlatAtBaseline) return ''

  let d = `M ${pts[0].x} ${yBaseline} L ${pts[0].x} ${pts[0].y}`
  for (let i = 1; i < pts.length; i++) {
    d += ` L ${pts[i].x} ${pts[i].y}`
  }
  d += ` L ${pts[pts.length - 1].x} ${yBaseline} Z`
  return d
}

const passLinePath = computed(() => chartLinear(renderedPassPoints.value))
const failLinePath = computed(() => chartLinear(renderedFailPoints.value))
const passAreaPath = computed(() => chartArea(renderedPassPoints.value))
const failAreaPath = computed(() => chartArea(renderedFailPoints.value))

function getPassDotStyle(pt, idx) {
  const transform = `translate(${pt.x}px, ${pt.y}px)`
  if (isInitialPhase.value) {
    const totalPts = Math.max(1, renderedPassPoints.value.length - 1)
    const delay = (idx / totalPts) * 800
    return {
      transform,
      animation: `dotPopIn 180ms cubic-bezier(0.34, 1.56, 0.64, 1) ${delay}ms both`
    }
  }
  return { transform }
}

function getFailDotStyle(pt, idx) {
  const transform = `translate(${pt.x}px, ${pt.y}px)`
  if (isInitialPhase.value) {
    const totalPts = Math.max(1, renderedPassPoints.value.length - 1)
    const delay = 120 + (idx / totalPts) * 800
    return {
      transform,
      animation: `dotPopIn 180ms cubic-bezier(0.34, 1.56, 0.64, 1) ${delay}ms both`
    }
  }
  return { transform }
}

const tooltipStyle = computed(() => {
  if (!hoveredChartPoint.value) return {}
  const x = hoveredChartPoint.value.x
  const y = hoveredChartPoint.value.y

  const xPercent = (x / 1000) * 100
  let translateX = '-50%'

  if (x > 800) {
    translateX = '-85%'
  } else if (x < 150) {
    translateX = '-15%'
  }

  return {
    left: `${xPercent}%`,
    top: `${(y / 270) * 100 - 6}%`,
    transform: `translate(${translateX}, -100%)`,
    zIndex: 9999,
  }
})

const tooltipArrowStyle = computed(() => {
  if (!hoveredChartPoint.value) return {}
  const x = hoveredChartPoint.value.x
  if (x > 800) return { left: '85%' }
  if (x < 150) return { left: '15%' }
  return { left: '50%' }
})

onMounted(() => {
  initialPhaseTimer = setTimeout(() => {
    isInitialPhase.value = false
  }, 1200)
})

onUnmounted(() => {
  clearTimeout(initialPhaseTimer)
  clearTimeout(initialDrawTimer)
  if (morphFrameId) cancelAnimationFrame(morphFrameId)
})

// ══════════════════════════════════════
// ── Teacher Ranking (Real BE Data)
// ══════════════════════════════════════
const topTeachers = ref([])

// ── Risk Students (Real BE Data) ──
const riskStudents = ref([])

// ── Pending Schedules (Real BE Data) ──
const pendingCount = computed(() => apiData.value?.pendingSchedules ?? 0)
const pendingScheduleItems = computed(() => {
  const items = apiData.value?.pendingScheduleItems
  return Array.isArray(items) ? items : []
})

// ── Announcements (Real BE Data) ──
const announcements = computed(() => {
  const logs = apiData.value?.recentAuditLogs
  if (!Array.isArray(logs)) return []
  return logs.slice(0, 3).map(log => ({
    title: log.action || 'Thông báo hệ thống',
    description: log.description || log.entity || 'Không có mô tả',
    time: formatRelativeTime(log.timestamp),
    icon: log.entity === 'academic' ? FileText : ShieldCheck,
    bgColor: log.entity === 'academic' ? 'bg-(--color-warning-bg)' : 'bg-(--color-info-bg)',
    iconColor: log.entity === 'academic' ? 'text-(--color-warning-text)' : 'text-(--color-info-text)'
  }))
})

function formatRelativeTime(value) {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return ''
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date)
}

// ══════════════════════════════════════
// ── Data Loading
// ══════════════════════════════════════
async function loadData() {
  loading.value = true
  error.value = null
  try {
    const secondaryRequests = Promise.all([
      bghApi.getEvaluationRanking().catch(() => null),
      bghApi.getPassFailFilterOptions(passFailFilterParams()).catch(() => null),
      bghApi.getPassFailRates({
        ...passFailFilterParams(),
        semesterId: selectedSemesterFilter.value,
      }).catch(() => null),
    ])
    const dashboardRes = await bghApi.getDashboard()
    apiData.value = unwrapApiData(dashboardRes)
    const riskItems = Array.isArray(apiData.value?.riskStudents) ? apiData.value.riskStudents : []
    riskStudents.value = riskItems.slice(0, 5).map(item => ({
      id: item.id,
      name: item.name || '',
      class: item.classCode || '',
      reason: `GPA ${Number(item.avgGpa || 0).toFixed(1)} · ${item.failCount ?? 0} môn chưa đạt`,
    }))

    // Hiển thị khung dashboard ngay khi dữ liệu thiết yếu đã sẵn sàng;
    // xếp hạng và biểu đồ tiếp tục tải song song vào đúng vị trí hiện có.
    loading.value = false

    const [rankingRes, filterRes, passFailRes] = await secondaryRequests

    const rankingData = rankingRes ? unwrapApiData(rankingRes) : null
    if (Array.isArray(rankingData) && rankingData.length) {
      topTeachers.value = rankingData.slice(0, 4).map(item => ({
        id: item.teacherId ?? item.id,
        name: item.teacherName || item.name || '',
        initials: (item.teacherName || item.name || 'GV').trim().split(/\s+/).pop().slice(0, 2).toUpperCase(),
        department: item.departmentName || item.dept || 'Chưa phân khoa',
        rating: Number(item.avgRating ?? item.avgScore ?? 0).toFixed(1),
        reviews: item.reviewCount ?? item.evals ?? 0,
      }))
    }

    if (filterRes) applyPassFailFilters(filterRes)
    if (passFailRes) passFailData.value = unwrapApiData(passFailRes)

    // Đánh dấu đã xong lần load đầu tiên -> Chuyển animation vẽ đường stroke thành CSS smooth morphing transition
    initialDrawTimer = setTimeout(() => {
      isInitialDrawFinished.value = true
    }, 1400)
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu tổng quan'
  } finally {
    loading.value = false
  }
}

onMounted(() => { loadData() })
</script>

<style scoped>
/* ── Entry Animations ── */
@keyframes kpi-card-in {
  from { opacity: 0; transform: translateY(16px); }
  to { opacity: 1; transform: translateY(0); }
}
.kpi-card-enter { animation: kpi-card-in 0.5s cubic-bezier(0.16, 1, 0.3, 1) both; }

@keyframes section-in {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
.section-enter { animation: section-in 0.6s cubic-bezier(0.16, 1, 0.3, 1) both; }

@keyframes banner-in {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}
.banner-enter { animation: banner-in 0.4s cubic-bezier(0.16, 1, 0.3, 1) both; }

@keyframes teacher-card-in {
  from { opacity: 0; transform: scale(0.96); }
  to { opacity: 1; transform: scale(1); }
}
.teacher-card-enter { animation: teacher-card-in 0.4s cubic-bezier(0.16, 1, 0.3, 1) both; }

/* ── Mini Chart Line Draw ── */
.mini-chart-line {
  stroke-dasharray: 600;
  stroke-dashoffset: 600;
  animation: draw-line 1.2s ease-out forwards;
}
@keyframes draw-line {
  to { stroke-dashoffset: 0; }
}

/* ════════════════════════════════════════════
   SENIOR MOTION DESIGNER SPECIFICATIONS
   ════════════════════════════════════════════ */

/* 1. Container Entry: opacity 0->1, translateY 10px->0, 280ms easeOutCubic */
.chart-container-enter {
  animation: chartContainerIn 280ms cubic-bezier(0.33, 1, 0.68, 1) both;
}
@keyframes chartContainerIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

/* 2. Soft Grid Lines: opacity 0->0.15, 200ms easeOut */
.chart-grid-line {
  animation: gridLineIn 200ms ease-out both;
}
@keyframes gridLineIn {
  from { opacity: 0; }
  to { opacity: 0.15; }
}

/* 3. Progressive Left-to-Right Pen Drawing (Initial Load) */
.chart-line-draw-pass {
  stroke-dasharray: 1500;
  stroke-dashoffset: 1500;
  animation: penDraw 800ms cubic-bezier(0.33, 1, 0.68, 1) 0ms forwards;
}

.chart-line-draw-fail {
  stroke-dasharray: 1500;
  stroke-dashoffset: 1500;
  animation: penDraw 800ms cubic-bezier(0.33, 1, 0.68, 1) 120ms forwards; /* 120ms Stagger */
}

@keyframes penDraw {
  to { stroke-dashoffset: 0; }
}

/* 4. Sequential Dot Pop-in: scale 0.6->1, opacity 0->1, 180ms easeOutBack(1.2) */
@keyframes dotPopIn {
  from {
    opacity: 0;
    transform: scale(0.6);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

/* 5. Area Fill Fade-In: opacity 0->1, 250ms, delayed 100ms AFTER line completes */
.chart-area-initial-pass {
  animation: areaFadeIn 250ms ease-out 900ms both; /* 800ms line + 100ms delay */
}

.chart-area-initial-fail {
  animation: areaFadeIn 250ms ease-out 1020ms both; /* 120ms + 800ms + 100ms delay */
}

.chart-area-morph {
  opacity: 1;
  transition: opacity 250ms ease-out;
}

@keyframes areaFadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

/* 6. Hover Crosshair & Tooltip */
.chart-crosshair {
  animation: crosshairFadeIn 100ms ease-out both;
}
@keyframes crosshairFadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.chart-tooltip-anim {
  animation: tooltipIn 150ms ease-out both;
}
@keyframes tooltipIn {
  from { opacity: 0; transform: translate(-50%, -100%) scale(0.97); }
  to { opacity: 1; transform: translate(-50%, -100%) scale(1); }
}

/* ── KPI Dot Pop ── */
@keyframes dot-pop {
  from { r: 0; opacity: 0; }
  to { r: 4; opacity: 1; }
}
.kpi-dot-pop { animation: dot-pop 0.2s ease-out both; }

/* ── Glowing Tooltip ── */
.glowing-tooltip {
  background: linear-gradient(135deg, #1e293b, #0f172a);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25), 0 0 8px rgba(59, 130, 246, 0.15);
}

.transition-all { transition-duration: 300ms; }
</style>
