<template>
  <div class="space-y-4 pb-12 flex flex-col">
    <!-- Header & Actions -->
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <div class="flex items-center gap-2">
          <div class="w-8 h-8 rounded-lg bg-indigo-500/10 text-indigo-600 dark:text-indigo-400 flex items-center justify-center font-bold">
            <ShieldCheck :size="18" />
          </div>
          <h1 class="text-xl font-bold text-heading">Danh Mục Vai Trò & Cơ Cấu Phân Quyền</h1>
        </div>
        <p class="text-xs text-muted mt-1">Quản lý danh mục vai trò hệ thống và cấu hình quyền hạn nghiệp vụ theo cơ sở</p>
      </div>

      <div class="flex items-center gap-2">
        <button
          @click="fetchRoles"
          class="flex items-center gap-1.5 px-3 py-2 bg-(--surface-input) hover:bg-(--surface-card) border border-card rounded-xl text-xs font-bold text-heading transition-all cursor-pointer"
        >
          <RefreshCw :size="14" :class="{ 'animate-spin': rolesLoading }" />
          <span>Làm mới</span>
        </button>
      </div>
    </div>

    <!-- Quick Stats Grid -->
    <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
      <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl bg-indigo-500/10 text-indigo-600 flex items-center justify-center font-bold">
          <Users :size="20" />
        </div>
        <div>
          <span class="text-[11px] text-muted font-medium block">Tổng thành viên</span>
          <strong class="text-base font-extrabold text-heading">{{ totalCampusMembers.toLocaleString('vi-VN') }}</strong>
        </div>
      </div>

      <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl bg-blue-500/10 text-blue-600 flex items-center justify-center font-bold">
          <ShieldCheck :size="20" />
        </div>
        <div>
          <span class="text-[11px] text-muted font-medium block">Vai trò hệ thống</span>
          <strong class="text-base font-extrabold text-heading">{{ rolesList.length }} vai trò</strong>
        </div>
      </div>

      <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl bg-teal-500/10 text-teal-600 flex items-center justify-center font-bold">
          <GraduationCap :size="20" />
        </div>
        <div>
          <span class="text-[11px] text-muted font-medium block">Giảng viên cơ sở</span>
          <strong class="text-base font-extrabold text-heading">{{ teacherCount }} giảng viên</strong>
        </div>
      </div>

      <div class="surface-card border border-card rounded-2xl p-4 flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl bg-amber-500/10 text-amber-600 flex items-center justify-center font-bold">
          <BookOpen :size="20" />
        </div>
        <div>
          <span class="text-[11px] text-muted font-medium block">Sinh viên cơ sở</span>
          <strong class="text-base font-extrabold text-heading">{{ studentCount.toLocaleString('vi-VN') }} sinh viên</strong>
        </div>
      </div>
    </div>

    <!-- Main Roles Section -->
    <div class="space-y-4">
      <!-- Search Filter -->
      <div class="surface-card border border-card rounded-2xl p-4 shadow-sm flex items-center gap-3">
        <div class="flex-1 min-w-[200px] relative">
          <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" :size="16" />
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Tìm kiếm theo tên hoặc mã code vai trò (vd: Giảng viên, Giáo vụ, hieu_truong)..."
            class="w-full pl-9 pr-3 py-2 bg-(--surface-input) border border-input rounded-xl text-xs text-body focus:outline-none focus:border-indigo-500"
          />
        </div>
      </div>

      <!-- Roles Table -->
      <div class="surface-card border border-card rounded-2xl shadow-sm overflow-hidden flex flex-col">
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs text-body whitespace-nowrap">
            <thead class="bg-(--surface-input)/60 border-b border-card font-bold text-heading uppercase tracking-wider text-[10px]">
              <tr>
                <th class="px-5 py-3.5 w-16 text-center">STT</th>
                <th class="px-5 py-3.5">Mã Vai Trò</th>
                <th class="px-5 py-3.5">Tên Vai Trò</th>
                <th class="px-5 py-3.5">Phân Loại</th>
                <th class="px-5 py-3.5 text-center">Số Lượng Thành Viên</th>
                <th class="px-5 py-3.5 text-right">Hành Động</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-card">
              <tr v-if="rolesLoading">
                <td colspan="6" class="py-12 text-center text-muted">
                  <Loader2 class="animate-spin mx-auto mb-2 text-indigo-500" :size="24" />
                  <span>Đang tải danh mục vai trò...</span>
                </td>
              </tr>
              <tr v-else-if="filteredRoles.length === 0">
                <td colspan="6" class="py-12 text-center text-muted">
                  <AlertCircle class="mx-auto mb-2 text-muted/50" :size="28" />
                  <span>Không tìm thấy vai trò nào phù hợp với từ khóa.</span>
                </td>
              </tr>
              <tr 
                v-for="(role, index) in filteredRoles" 
                :key="role.maVaiTro || role.id"
                class="hover:bg-(--surface-input)/40 transition-colors"
              >
                <td class="px-5 py-4 text-center font-bold text-muted">{{ index + 1 }}</td>
                <td class="px-5 py-4">
                  <span class="inline-flex items-center px-2.5 py-1 rounded-lg text-xs font-mono font-bold bg-indigo-500/10 text-indigo-600 dark:text-indigo-400 border border-indigo-500/20">
                    {{ role.maCode || role.maCodeVaiTro }}
                  </span>
                </td>
                <td class="px-5 py-4">
                  <div class="flex items-center gap-2.5">
                    <span class="p-1.5 rounded-lg bg-(--surface-input) text-heading">
                      <component :is="getRoleIcon(role.maCode || role.maCodeVaiTro)" :size="16" />
                    </span>
                    <strong class="text-heading text-xs font-bold">{{ role.tenVaiTro }}</strong>
                  </div>
                </td>
                <td class="px-5 py-4">
                  <span class="text-xs text-muted font-medium">
                    {{ getRoleCategory(role.maCode || role.maCodeVaiTro) }}
                  </span>
                </td>
                <td class="px-5 py-4 text-center">
                  <span 
                    class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold"
                    :class="(role.memberCount || 0) > 0 ? 'bg-blue-500/10 text-blue-600 dark:text-blue-400' : 'bg-slate-500/10 text-muted'"
                  >
                    <Users :size="12" />
                    {{ (role.memberCount || 0).toLocaleString('vi-VN') }} thành viên
                  </span>
                </td>
                <td class="px-5 py-4 text-right">
                  <div class="inline-flex items-center gap-2">
                    <!-- If System Role, Lock QuyenHan for BGH -->
                    <span 
                      v-if="isSystemAdminRole(role.maCode || role.maCodeVaiTro)"
                      class="px-2.5 py-1 rounded-xl text-[11px] font-bold bg-amber-500/10 text-amber-600 border border-amber-500/20"
                    >
                      Quản trị hệ thống
                    </span>
                    <button
                      v-else
                      @click="openPermissionMatrix(role)"
                      class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-xs font-bold shadow-sm transition-all cursor-pointer"
                    >
                      <SlidersHorizontal :size="13" />
                      <span>Quyền hạn</span>
                    </button>

                    <button
                      @click="openRoleMembers(role)"
                      class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-bold shadow-sm transition-all cursor-pointer"
                    >
                      <Eye :size="13" />
                      <span>Xem thành viên</span>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- 1. MEMBER DRAWER / MODAL -->
    <Teleport to="body">
      <div v-if="selectedRole" class="fixed inset-0 z-50 flex justify-end">
        <div 
          class="fixed inset-0 bg-black/40 backdrop-blur-xs transition-opacity"
          @click="closeMembersModal"
        />

        <div class="relative w-full max-w-2xl bg-(--surface-modal) h-full shadow-2xl border-l border-card flex flex-col z-10">
          <div class="p-5 border-b border-card bg-(--surface-card) flex items-center justify-between">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-xl bg-indigo-500/10 text-indigo-600 flex items-center justify-center font-bold">
                <component :is="getRoleIcon(selectedRole.maCode || selectedRole.maCodeVaiTro)" :size="20" />
              </div>
              <div>
                <h3 class="text-sm font-bold text-heading flex items-center gap-2">
                  <span>{{ selectedRole.tenVaiTro }}</span>
                  <span class="text-xs px-2 py-0.5 rounded bg-indigo-500/10 text-indigo-600 font-mono">
                    {{ selectedRole.maCode || selectedRole.maCodeVaiTro }}
                  </span>
                </h3>
                <p class="text-xs text-muted mt-0.5">
                  Tổng số: {{ memberTotal.toLocaleString('vi-VN') }} tài khoản thuộc cơ sở
                </p>
              </div>
            </div>

            <button 
              @click="closeMembersModal"
              class="p-2 rounded-xl text-muted hover:text-heading hover:bg-(--surface-input) transition-all cursor-pointer"
            >
              <X :size="18" />
            </button>
          </div>

          <div class="p-4 border-b border-card bg-(--surface-card)/50 flex items-center gap-3">
            <div class="flex-1 relative">
              <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" :size="15" />
              <input
                v-model="memberSearch"
                @input="debounceSearch"
                type="text"
                placeholder="Tìm kiếm theo tên, email, tài khoản..."
                class="w-full pl-9 pr-3 py-1.5 bg-(--surface-input) border border-input rounded-xl text-xs text-body focus:outline-none focus:border-indigo-500"
              />
            </div>
            <span class="text-xs text-muted font-medium whitespace-nowrap">
              Trang {{ memberPage }} / {{ totalPages || 1 }}
            </span>
          </div>

          <div class="flex-1 overflow-y-auto p-4">
            <div v-if="membersLoading" class="py-16 text-center text-muted">
              <Loader2 class="animate-spin mx-auto mb-2 text-indigo-500" :size="24" />
              <span class="text-xs">Đang tải danh sách thành viên...</span>
            </div>

            <div v-else-if="membersList.length === 0" class="py-16 text-center text-muted">
              <Users class="mx-auto mb-2 text-muted/40" :size="32" />
              <p class="text-xs font-semibold">Không tìm thấy thành viên nào</p>
              <p class="text-[11px] text-muted mt-1">Thử tìm kiếm với từ khóa khác</p>
            </div>

            <div v-else class="space-y-2">
              <div
                v-for="u in membersList"
                :key="u.id"
                class="p-3 rounded-xl surface-card border border-card hover:border-indigo-500/40 transition-all flex items-center justify-between gap-3"
              >
                <div class="flex items-center gap-3 min-w-0">
                  <div class="w-8 h-8 rounded-full bg-gradient-to-br from-indigo-500/20 to-blue-500/20 text-indigo-600 font-bold flex items-center justify-center text-xs shrink-0 border border-indigo-500/20">
                    {{ (u.name || 'U').charAt(0).toUpperCase() }}
                  </div>
                  <div class="min-w-0">
                    <strong class="text-xs font-bold text-heading block truncate">{{ u.name }}</strong>
                    <span class="text-[11px] text-muted block truncate">{{ u.email }}</span>
                  </div>
                </div>

                <div class="flex items-center gap-2 shrink-0">
                  <span
                    class="px-2 py-0.5 rounded-full text-[10px] font-bold"
                    :class="u.status === 'hoat_dong' ? 'bg-emerald-500/10 text-emerald-600' : 'bg-rose-500/10 text-rose-600'"
                  >
                    {{ u.status === 'hoat_dong' ? 'Hoạt động' : 'Đã khóa' }}
                  </span>

                  <button
                    v-if="(selectedRole.maCode || selectedRole.maCodeVaiTro) === 'giao_vien'"
                    @click="router.push(`/bgh/human-resources/${u.id}`); closeMembersModal()"
                    class="px-2.5 py-1 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-[10px] font-bold transition-all cursor-pointer"
                  >
                    Hồ sơ GV
                  </button>
                </div>
              </div>
            </div>
          </div>

          <div class="p-4 border-t border-card bg-(--surface-card) flex items-center justify-between">
            <span class="text-xs text-muted">
              Hiển thị {{ membersList.length }} / {{ memberTotal }} thành viên
            </span>

            <div class="flex items-center gap-2">
              <button
                @click="changePage(memberPage - 1)"
                :disabled="memberPage <= 1 || membersLoading"
                class="px-3 py-1.5 rounded-lg border border-card text-xs font-bold text-heading disabled:opacity-40 disabled:cursor-not-allowed hover:bg-(--surface-input) cursor-pointer"
              >
                Trước
              </button>
              <span class="text-xs font-bold text-heading px-2">{{ memberPage }}</span>
              <button
                @click="changePage(memberPage + 1)"
                :disabled="memberPage >= totalPages || membersLoading"
                class="px-3 py-1.5 rounded-lg border border-card text-xs font-bold text-heading disabled:opacity-40 disabled:cursor-not-allowed hover:bg-(--surface-input) cursor-pointer"
              >
                Sau
              </button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- 2. PERMISSION MATRIX DRAWER / MODAL -->
    <Teleport to="body">
      <div v-if="selectedPermRole" class="fixed inset-0 z-50 flex justify-end">
        <div 
          class="fixed inset-0 bg-black/40 backdrop-blur-xs transition-opacity"
          @click="closePermissionModal"
        />

        <div class="relative w-full max-w-3xl bg-(--surface-modal) h-full shadow-2xl border-l border-card flex flex-col z-10">
          <!-- Drawer Header -->
          <div class="p-5 border-b border-card bg-(--surface-card) flex items-center justify-between">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-xl bg-emerald-500/10 text-emerald-600 flex items-center justify-center font-bold">
                <SlidersHorizontal :size="20" />
              </div>
              <div>
                <h3 class="text-sm font-bold text-heading flex items-center gap-2">
                  <span>Phân Quyền Hạn — Vai Trò: {{ selectedPermRole.tenVaiTro }}</span>
                  <span class="text-xs px-2 py-0.5 rounded bg-emerald-500/10 text-emerald-600 font-mono">
                    {{ selectedPermRole.maCode || selectedPermRole.maCodeVaiTro }}
                  </span>
                </h3>
                <p class="text-xs text-muted mt-0.5">
                  Cấu hình chi tiết các quyền hạn nghiệp vụ áp dụng cho cơ sở
                </p>
              </div>
            </div>

            <button 
              @click="closePermissionModal"
              class="p-2 rounded-xl text-muted hover:text-heading hover:bg-(--surface-input) transition-all cursor-pointer"
            >
              <X :size="18" />
            </button>
          </div>

          <!-- Role Safety Warning for Student / Parent / Teacher -->
          <div v-if="isStudentOrParentRole" class="p-3.5 bg-amber-500/10 border-b border-amber-500/20 flex items-center gap-2.5 text-xs text-amber-700 dark:text-amber-300">
            <ShieldAlert :size="16" class="shrink-0 text-amber-600" />
            <span><strong>Rào chắn an toàn học vụ:</strong> Vai trò Người học / Phụ huynh chỉ được phép gán các quyền Xem thông tin cá nhân và Gửi đơn từ. Toàn bộ các quyền Phê duyệt & Quản trị đã được khóa an toàn.</span>
          </div>
          <div v-else-if="isTeacherRole" class="p-3.5 bg-blue-500/10 border-b border-blue-500/20 flex items-center gap-2.5 text-xs text-blue-700 dark:text-blue-300">
            <ShieldAlert :size="16" class="shrink-0 text-blue-600" />
            <span><strong>Rào chắn an toàn học vụ:</strong> Vai trò Giảng viên chỉ được phép Xem lịch dạy, Nhập/Chấm điểm và Tiếp nhận/Xử lý đơn từ của lớp phụ trách. Các quyền nhạy cảm (Tạo/Sửa môn học, Tạo/Xếp lịch học, Tạo đề thi/Ngân hàng câu hỏi của Giáo vụ) đã được khóa an toàn.</span>
          </div>

          <!-- Quick Action Toolbar inside Permission Drawer -->
          <div class="p-4 border-b border-card bg-(--surface-card)/60 flex flex-wrap items-center justify-between gap-3">
            <div class="flex-1 min-w-[200px] relative">
              <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" :size="15" />
              <input
                v-model="permSearch"
                type="text"
                placeholder="Tìm kiếm quyền hạn (vd: điểm, lịch, môn học)..."
                class="w-full pl-9 pr-3 py-1.5 bg-(--surface-input) border border-input rounded-xl text-xs text-body focus:outline-none focus:border-emerald-500"
              />
            </div>

            <div class="flex items-center gap-2">
              <button
                @click="resetToDefaultPermissions"
                class="px-2.5 py-1.5 rounded-lg border border-card bg-(--surface-input) hover:bg-(--surface-card) text-amber-600 dark:text-amber-400 text-[11px] font-bold transition-all cursor-pointer flex items-center gap-1"
                title="Khôi phục về bộ quyền chuẩn khuyến nghị của Nhà trường"
              >
                <RotateCcw :size="12" />
                <span>Khôi phục mặc định</span>
              </button>
              <button
                v-if="!isStudentOrParentRole"
                @click="toggleAllPermissions(true)"
                class="px-2.5 py-1.5 rounded-lg border border-card bg-(--surface-input) hover:bg-(--surface-card) text-heading text-[11px] font-bold transition-all cursor-pointer"
              >
                Bật tất cả
              </button>
              <button
                @click="toggleAllPermissions(false)"
                class="px-2.5 py-1.5 rounded-lg border border-card bg-(--surface-input) hover:bg-(--surface-card) text-heading text-[11px] font-bold transition-all cursor-pointer"
              >
                Tắt tất cả
              </button>
            </div>
          </div>

          <!-- Permission Matrix Content Area -->
          <div class="flex-1 overflow-y-auto p-5 space-y-6">
            <div v-if="permsLoading" class="py-20 text-center text-muted">
              <Loader2 class="animate-spin mx-auto mb-2 text-emerald-500" :size="28" />
              <span class="text-xs">Đang tải ma trận phân quyền từ cơ sở dữ liệu...</span>
            </div>

            <div v-else-if="filteredPermissionCatalog.length === 0" class="py-20 text-center text-muted">
              <AlertCircle class="mx-auto mb-2 text-muted/50" :size="32" />
              <span class="text-xs">Không tìm thấy quyền hạn nào phù hợp.</span>
            </div>

            <div 
              v-else 
              v-for="moduleGroup in filteredPermissionCatalog" 
              :key="moduleGroup.moduleKey"
              class="surface-card border border-card rounded-2xl p-4 shadow-sm space-y-3"
            >
              <!-- Module Header -->
              <div class="flex items-center justify-between pb-2 border-b border-card">
                <div class="flex items-center gap-2">
                  <div class="w-6 h-6 rounded-lg bg-emerald-500/10 text-emerald-600 flex items-center justify-center font-bold text-xs">
                    <component :is="getModuleIcon(moduleGroup.moduleKey)" :size="14" />
                  </div>
                  <h4 class="text-xs font-bold text-heading">{{ moduleGroup.moduleName }}</h4>
                </div>

                <div class="flex items-center gap-2">
                  <span class="text-[11px] text-muted font-medium">
                    {{ getSelectedCountInModule(moduleGroup) }} / {{ moduleGroup.permissions.length }} đã chọn
                  </span>
                  <button
                    v-if="!isStudentOrParentRole"
                    @click="toggleModuleGroup(moduleGroup)"
                    class="text-[11px] font-bold text-emerald-600 hover:text-emerald-700 cursor-pointer"
                  >
                    {{ isModuleAllSelected(moduleGroup) ? 'Bỏ chọn nhóm' : 'Chọn nhóm' }}
                  </button>
                </div>
              </div>

              <!-- Module Permissions List -->
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-2.5 pt-1">
                <label
                  v-for="perm in moduleGroup.permissions"
                  :key="perm.code"
                  class="p-3 rounded-xl border transition-all flex items-start gap-3 select-none"
                  :class="[
                    isPermDisabled(perm) 
                      ? 'opacity-50 cursor-not-allowed bg-slate-500/5 border-dashed border-card' 
                      : 'cursor-pointer',
                    assignedPerms.has(perm.code) && !isPermDisabled(perm)
                      ? 'border-emerald-500/40 bg-emerald-500/5' 
                      : (!isPermDisabled(perm) ? 'border-card bg-(--surface-input)/30 hover:border-emerald-500/20' : '')
                  ]"
                >
                  <input
                    type="checkbox"
                    :checked="assignedPerms.has(perm.code)"
                    :disabled="isPermDisabled(perm)"
                    @change="togglePerm(perm.code)"
                    class="mt-0.5 w-4 h-4 rounded text-emerald-600 border-card focus:ring-emerald-500 accent-emerald-600 disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer"
                  />
                  <div class="flex-1 min-w-0">
                    <div class="flex items-center gap-2">
                      <strong class="text-xs font-bold text-heading block truncate">{{ perm.name }}</strong>
                      <span 
                        class="px-1.5 py-0.5 rounded text-[9px] font-bold shrink-0 uppercase"
                        :class="getActionBadgeClass(perm.action)"
                      >
                        {{ perm.action }}
                      </span>
                      <span v-if="isPermDisabled(perm)" class="inline-flex items-center gap-0.5 text-[9px] text-amber-600 dark:text-amber-400 font-bold ml-auto" title="Khóa an toàn">
                        <Lock :size="10" /> Khóa
                      </span>
                    </div>
                    <span class="text-[11px] font-mono text-muted block mt-0.5">{{ perm.code }}</span>
                    <p v-if="perm.description" class="text-[11px] text-muted mt-1 leading-relaxed">
                      {{ perm.description }}
                    </p>
                  </div>
                </label>
              </div>
            </div>
          </div>

          <!-- Drawer Footer with Save & Notification -->
          <div class="p-4 border-t border-card bg-(--surface-card) flex items-center justify-between">
            <div class="flex items-center gap-2">
              <span class="text-xs text-muted">
                Đã cấp: <strong class="text-heading">{{ assignedPerms.size }}</strong> / {{ totalCatalogPerms }} quyền
              </span>
              <span v-if="saveSuccessMsg" class="text-xs text-emerald-600 font-bold flex items-center gap-1 animate-pulse">
                <Check :size="14" /> {{ saveSuccessMsg }}
              </span>
            </div>

            <div class="flex items-center gap-2">
              <button
                @click="closePermissionModal"
                class="px-4 py-2 rounded-xl border border-card text-xs font-bold text-heading hover:bg-(--surface-input) transition-all cursor-pointer"
              >
                Đóng
              </button>
              <button
                @click="saveRolePermissions"
                :disabled="permsSaving"
                class="flex items-center gap-1.5 px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-xs font-bold shadow-sm transition-all disabled:opacity-50 cursor-pointer"
              >
                <Loader2 v-if="permsSaving" class="animate-spin" :size="14" />
                <Save v-else :size="14" />
                <span>{{ permsSaving ? 'Đang lưu...' : 'Lưu thay đổi' }}</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  ShieldCheck, Users, GraduationCap, BookOpen, Search, Loader2,
  RefreshCw, Eye, X, UserCheck, AlertCircle, User, SlidersHorizontal,
  Save, Check, Calendar, FileText, BarChart, Award, ShieldAlert, Lock, RotateCcw
} from 'lucide-vue-next'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const router = useRouter()

// Roles State
const rolesLoading = ref(false)
const rolesList = ref([])
const searchQuery = ref('')

// Role Members Drawer State
const selectedRole = ref(null)
const membersLoading = ref(false)
const membersList = ref([])
const memberSearch = ref('')
const memberPage = ref(1)
const memberPageSize = ref(10)
const memberTotal = ref(0)
let searchTimeout = null

// Permission Matrix Drawer State
const selectedPermRole = ref(null)
const permsLoading = ref(false)
const permsSaving = ref(false)
const permissionCatalog = ref([])
const assignedPerms = ref(new Set())
const permSearch = ref('')
const saveSuccessMsg = ref('')

const defaultRolePresets = {
  hieu_truong: [
    'training.read', 'training.manage_curriculum',
    'schedules.read', 'schedules.approve',
    'exams.read', 'exams.unlock_grade',
    'requests.read', 'requests.process',
    'reports.read', 'reports.export', 'reports.ai_analysis'
  ],
  nhan_vien: [
    'training.read', 'training.create', 'training.update',
    'schedules.read', 'schedules.create', 'schedules.update',
    'exams.read', 'exams.create',
    'requests.read', 'requests.update', 'requests.process',
    'reports.read', 'reports.export'
  ],
  giao_vien: [
    'training.read',
    'schedules.read',
    'exams.read', 'exams.update', 'exams.grade',
    'requests.read', 'requests.update', 'requests.create',
    'reports.read'
  ],
  hoc_sinh: [
    'training.read',
    'schedules.read',
    'exams.read',
    'requests.read', 'requests.create'
  ],
  phu_huynh: [
    'training.read',
    'schedules.read',
    'exams.read',
    'reports.read'
  ]
}

const SENSITIVE_TEACHER_PERMISSIONS = [
  'training.create',
  'training.update',
  'training.delete',
  'training.manage_curriculum',
  'schedules.create',
  'schedules.update',
  'schedules.delete',
  'schedules.approve',
  'exams.create',
  'exams.delete',
  'exams.unlock_grade',
  'requests.delete',
  'reports.ai_analysis'
]

const totalCampusMembers = computed(() => {
  return rolesList.value.reduce((sum, r) => sum + (r.memberCount || 0), 0)
})

const teacherCount = computed(() => {
  const t = rolesList.value.find(r => (r.maCode || r.maCodeVaiTro) === 'giao_vien')
  return t ? (t.memberCount || 0) : 0
})

const studentCount = computed(() => {
  const s = rolesList.value.find(r => (r.maCode || r.maCodeVaiTro) === 'hoc_sinh')
  return s ? (s.memberCount || 0) : 0
})

const totalPages = computed(() => {
  return Math.ceil(memberTotal.value / memberPageSize.value)
})

const totalCatalogPerms = computed(() => {
  return permissionCatalog.value.reduce((sum, g) => sum + g.permissions.length, 0)
})

const isStudentOrParentRole = computed(() => {
  const code = selectedPermRole.value?.maCode || selectedPermRole.value?.maCodeVaiTro || ''
  return code === 'hoc_sinh' || code === 'phu_huynh'
})

const isTeacherRole = computed(() => {
  const code = selectedPermRole.value?.maCode || selectedPermRole.value?.maCodeVaiTro || ''
  return code === 'giao_vien'
})

function isSystemAdminRole(code) {
  return code === 'sieu_quan_tri' || code === 'quan_tri'
}

function isPermDisabled(perm) {
  const code = selectedPermRole.value?.maCode || selectedPermRole.value?.maCodeVaiTro || ''
  if (code === 'hoc_sinh' || code === 'phu_huynh') {
    return perm.action !== 'read' && perm.code !== 'requests.create'
  }
  if (code === 'giao_vien') {
    return SENSITIVE_TEACHER_PERMISSIONS.includes(perm.code)
  }
  return false
}

function resetToDefaultPermissions() {
  const code = selectedPermRole.value?.maCode || selectedPermRole.value?.maCodeVaiTro || ''
  const preset = defaultRolePresets[code] || []
  assignedPerms.value = new Set(preset)
}

function getRoleIcon(code) {
  switch (code) {
    case 'hieu_truong':
    case 'quan_tri_co_so':
      return ShieldCheck
    case 'giao_vien':
      return GraduationCap
    case 'hoc_sinh':
      return BookOpen
    case 'nhan_vien':
      return UserCheck
    default:
      return User
  }
}

function getRoleCategory(code) {
  switch (code) {
    case 'hieu_truong':
    case 'quan_tri_co_so':
      return 'Ban Lãnh Đạo & Quản Trị'
    case 'nhan_vien':
      return 'Cán Bộ Giáo Vụ & Đào Tạo'
    case 'giao_vien':
      return 'Giảng Viên Trực Thuộc'
    case 'hoc_sinh':
      return 'Sinh Viên / Người Học'
    case 'phu_huynh':
      return 'Phụ Huynh Học Sinh'
    case 'hoidong_quanly_noidung':
      return 'Hội Đồng Nội Dung'
    default:
      return 'Khác'
  }
}

function getModuleIcon(moduleKey) {
  switch (moduleKey) {
    case 'training':
      return BookOpen
    case 'schedules':
      return Calendar
    case 'exams':
      return Award
    case 'requests':
      return FileText
    case 'reports':
      return BarChart
    default:
      return ShieldCheck
  }
}

function getActionBadgeClass(action) {
  switch (action) {
    case 'read':
      return 'bg-blue-500/10 text-blue-600'
    case 'create':
      return 'bg-emerald-500/10 text-emerald-600'
    case 'update':
      return 'bg-amber-500/10 text-amber-600'
    case 'delete':
      return 'bg-rose-500/10 text-rose-600'
    case 'approve':
      return 'bg-purple-500/10 text-purple-600'
    case 'export':
      return 'bg-teal-500/10 text-teal-600'
    default:
      return 'bg-slate-500/10 text-muted'
  }
}

async function fetchRoles() {
  rolesLoading.value = true
  try {
    const res = await bghApi.getRoles()
    rolesList.value = unwrapApiData(res) || []
  } catch (err) {
    console.error('Lỗi tải danh mục vai trò:', err)
  } finally {
    rolesLoading.value = false
  }
}

const filteredRoles = computed(() => {
  if (!searchQuery.value) return rolesList.value
  const query = searchQuery.value.toLowerCase().trim()
  return rolesList.value.filter(r => 
    (r.tenVaiTro && r.tenVaiTro.toLowerCase().includes(query)) ||
    (r.maCodeVaiTro && r.maCodeVaiTro.toLowerCase().includes(query)) ||
    (r.maCode && r.maCode.toLowerCase().includes(query))
  )
})

// === Members Modal Handlers ===
async function openRoleMembers(role) {
  selectedRole.value = role
  memberPage.value = 1
  memberSearch.value = ''
  await fetchRoleMembers()
}

function closeMembersModal() {
  selectedRole.value = null
  membersList.value = []
  memberTotal.value = 0
}

async function fetchRoleMembers() {
  if (!selectedRole.value) return
  membersLoading.value = true
  try {
    const roleCode = selectedRole.value.maCode || selectedRole.value.maCodeVaiTro
    const res = await bghApi.getRoleMembers(roleCode, {
      search: memberSearch.value,
      page: memberPage.value,
      pageSize: memberPageSize.value
    })
    const data = unwrapApiData(res)
    membersList.value = data?.items || []
    memberTotal.value = data?.total || 0
  } catch (err) {
    console.error('Lỗi khi tải thành viên của vai trò:', err)
    membersList.value = []
    memberTotal.value = 0
  } finally {
    membersLoading.value = false
  }
}

function debounceSearch() {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    memberPage.value = 1
    fetchRoleMembers()
  }, 350)
}

function changePage(newPage) {
  if (newPage < 1 || newPage > totalPages.value) return
  memberPage.value = newPage
  fetchRoleMembers()
}

// === Permission Matrix Handlers ===
const filteredPermissionCatalog = computed(() => {
  if (!permSearch.value) return permissionCatalog.value
  const q = permSearch.value.toLowerCase().trim()
  return permissionCatalog.value
    .map(g => {
      const perms = g.permissions.filter(p => 
        p.name.toLowerCase().includes(q) ||
        p.code.toLowerCase().includes(q) ||
        (p.description && p.description.toLowerCase().includes(q))
      )
      return { ...g, permissions: perms }
    })
    .filter(g => g.permissions.length > 0)
})

async function openPermissionMatrix(role) {
  selectedPermRole.value = role
  permSearch.value = ''
  saveSuccessMsg.value = ''
  permsLoading.value = true

  try {
    // 1. Fetch catalog
    if (permissionCatalog.value.length === 0) {
      const catRes = await bghApi.getPermissionsCatalog()
      const data = unwrapApiData(catRes) || catRes?.data || []
      permissionCatalog.value = Array.isArray(data) ? data : []
    }

    // 2. Fetch role assigned permissions
    const roleCode = role.maCode || role.maCodeVaiTro
    const rolePermsRes = await bghApi.getRolePermissions(roleCode)
    const roleData = unwrapApiData(rolePermsRes) || rolePermsRes?.data || {}
    const codes = roleData?.permissionCodes || []
    assignedPerms.value = new Set(codes)
  } catch (err) {
    console.error('Lỗi khi tải danh mục quyền hạn:', err)
    assignedPerms.value = new Set()
  } finally {
    permsLoading.value = false
  }
}

function closePermissionModal() {
  selectedPermRole.value = null
  saveSuccessMsg.value = ''
}

function togglePerm(code) {
  const perm = permissionCatalog.value.flatMap(g => g.permissions).find(p => p.code === code)
  if (perm && isPermDisabled(perm)) return

  if (assignedPerms.value.has(code)) {
    assignedPerms.value.delete(code)
  } else {
    assignedPerms.value.add(code)
  }
}

function getSelectedCountInModule(moduleGroup) {
  return moduleGroup.permissions.filter(p => assignedPerms.value.has(p.code)).length
}

function isModuleAllSelected(moduleGroup) {
  return moduleGroup.permissions.every(p => assignedPerms.value.has(p.code))
}

function toggleModuleGroup(moduleGroup) {
  const allSelected = isModuleAllSelected(moduleGroup)
  moduleGroup.permissions.forEach(p => {
    if (isPermDisabled(p)) return
    if (allSelected) {
      assignedPerms.value.delete(p.code)
    } else {
      assignedPerms.value.add(p.code)
    }
  })
}

function toggleAllPermissions(enable) {
  if (enable) {
    permissionCatalog.value.forEach(g => {
      g.permissions.forEach(p => {
        if (!isPermDisabled(p)) {
          assignedPerms.value.add(p.code)
        }
      })
    })
  } else {
    assignedPerms.value.clear()
  }
}

async function saveRolePermissions() {
  if (!selectedPermRole.value) return
  permsSaving.value = true
  saveSuccessMsg.value = ''

  try {
    const roleCode = selectedPermRole.value.maCode || selectedPermRole.value.maCodeVaiTro
    const payload = {
      permissionCodes: Array.from(assignedPerms.value)
    }
    const res = await bghApi.updateRolePermissions(roleCode, payload)
    const updated = unwrapApiData(res)
    assignedPerms.value = new Set(updated?.permissionCodes || payload.permissionCodes)
    saveSuccessMsg.value = 'Đã lưu thay đổi vào CSDL!'
    setTimeout(() => {
      saveSuccessMsg.value = ''
    }, 3000)
  } catch (err) {
    console.error('Lỗi khi lưu quyền hạn:', err)
    alert('Lỗi: ' + (err.message || err))
  } finally {
    permsSaving.value = false
  }
}

onMounted(() => {
  fetchRoles()
})
</script>
