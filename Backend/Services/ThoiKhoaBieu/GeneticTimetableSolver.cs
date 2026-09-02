using Backend.Configuration;
using Backend.DTOs.SmartTimetable.Suggestions;
using Backend.Models;
using Backend.Services.ThoiKhoaBieu.Scoring;
using Microsoft.Extensions.Options;

namespace Backend.Services.ThoiKhoaBieu;

public sealed class GenerationProgress
{
    public int TheHeHienTai { get; set; }
    public int TongTheHe { get; set; }
    public double BestFitness { get; set; }
    public int XepDuoc { get; set; }
    public int KhongXepDuoc { get; set; }
    public double? ThoiGianChayMs { get; set; }
}

public sealed class TimetableAssignment
{
    public int MaKhoaHoc { get; set; }
    public int MaGiaoVien { get; set; }
    public string? TenGiaoVien { get; set; }
    public int? MucDoPhuHop { get; set; }
    public int ThuTrongTuan { get; set; }
    public int MaCaHoc { get; set; }
    public int MaPhong { get; set; }
    public double Score { get; set; }
    public ScheduleSlotScoreComponentsDto Components { get; set; } = new();
    public List<string> Reasons { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public sealed class GeneticTimetableResult
{
    public List<TimetableAssignment> Assignments { get; set; } = new();
    public int XepDuoc { get; set; }
    public int KhongXepDuoc { get; set; }
    public double BestFitness { get; set; }
    public int TheHeDaChay { get; set; }
    public double ThoiGianChayMs { get; set; }
}

public interface IGeneticTimetableSolver
{
    GeneticTimetableResult Solve(
        IReadOnlyList<KhoaHoc> courses,
        IReadOnlyList<Backend.Models.CaHoc> shifts,
        IReadOnlyList<PhongHoc> rooms,
        IReadOnlyDictionary<int, int> requiredSlotsPerCourse,
        IReadOnlyDictionary<int, int> studentCounts,
        IReadOnlyDictionary<int, IReadOnlySet<(int Day, int Shift)>> confirmedAvailabilityByTeacher,
        int tongTheHe,
        int kichThuocQuanThe,
        double tyLeCheo,
        int doTuoiThoToiDa,
        Action<GenerationProgress>? onProgress = null);
}

public sealed class GeneticTimetableSolver : IGeneticTimetableSolver
{
    private readonly IScheduleCandidateScoringService _scoringService;
    private readonly SmartTimetableScoringOptions _options;
    private readonly Random _random = new(20260701);

    private static readonly int[] WeekDays = { 2, 3, 4, 5, 6, 7 };

    public GeneticTimetableSolver(
        IScheduleCandidateScoringService scoringService,
        IOptions<SmartTimetableScoringOptions> options)
    {
        _scoringService = scoringService;
        _options = options.Value;
    }

    public GeneticTimetableResult Solve(
        IReadOnlyList<KhoaHoc> courses,
        IReadOnlyList<Backend.Models.CaHoc> shifts,
        IReadOnlyList<PhongHoc> rooms,
        IReadOnlyDictionary<int, int> requiredSlotsPerCourse,
        IReadOnlyDictionary<int, int> studentCounts,
        IReadOnlyDictionary<int, IReadOnlySet<(int Day, int Shift)>> confirmedAvailabilityByTeacher,
        int tongTheHe,
        int kichThuocQuanThe,
        double tyLeCheo,
        int doTuoiThoToiDa,
        Action<GenerationProgress>? onProgress = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        tongTheHe = Math.Clamp(tongTheHe, 1, 1000);
        kichThuocQuanThe = Math.Clamp(kichThuocQuanThe, 10, 200);
        tyLeCheo = Math.Clamp(tyLeCheo, 0.0, 1.0);
        doTuoiThoToiDa = Math.Clamp(doTuoiThoToiDa, 1, 100);

        var problem = BuildProblem(courses, shifts, rooms, requiredSlotsPerCourse, studentCounts, confirmedAvailabilityByTeacher);
        if (problem.Courses.Count == 0)
            return new GeneticTimetableResult
            {
                BestFitness = 0,
                TheHeDaChay = 0,
                ThoiGianChayMs = 0,
                KhongXepDuoc = problem.UnassignableCourseIds.Count
            };

        var population = Initialize(problem, kichThuocQuanThe);
        var best = population[0].Clone();
        var bestFitness = double.NegativeInfinity;
        var stale = 0;
        var generationsUsed = 0;

        for (var generation = 1; generation <= tongTheHe; generation++)
        {
            generationsUsed = generation;

            var sorted = population.OrderByDescending(c => c.Fitness).ToList();
            if (sorted[0].Fitness > bestFitness)
            {
                bestFitness = sorted[0].Fitness;
                best = sorted[0].Clone();
                stale = 0;
            }
            else
            {
                stale++;
            }

            var next = new List<Chromosome>(kichThuocQuanThe) { sorted[0].Clone(), sorted[1].Clone() };

            while (next.Count < kichThuocQuanThe)
            {
                var parentA = TournamentSelect(sorted, 3);
                var parentB = TournamentSelect(sorted, 3);

                var (childA, childB) = Crossover(problem, parentA, parentB, tyLeCheo);
                Mutate(problem, childA, tyLeCheo);
                Mutate(problem, childB, tyLeCheo);

                Evaluate(problem, childA);
                Evaluate(problem, childB);

                next.Add(childA);
                if (next.Count < kichThuocQuanThe)
                    next.Add(childB);
            }

            population = next;

            var bestNow = population.OrderByDescending(c => c.Fitness).First();
            var assignedCourses = CountAssignedCourses(problem, bestNow);
            onProgress?.Invoke(new GenerationProgress
            {
                TheHeHienTai = generation,
                TongTheHe = tongTheHe,
                BestFitness = bestNow.Fitness,
                XepDuoc = assignedCourses,
                KhongXepDuoc = problem.Courses.Count - assignedCourses,
                ThoiGianChayMs = stopwatch.Elapsed.TotalMilliseconds
            });

            if (stale >= doTuoiThoToiDa)
                break;
        }

        stopwatch.Stop();

        // Final repair: guarantee conflict-free output
        RepairGreedy(problem, best);

        var (assignments, xepDuoc, khongXepDuoc) = Decode(problem, best);

        return new GeneticTimetableResult
        {
            Assignments = assignments,
            XepDuoc = xepDuoc,
            KhongXepDuoc = khongXepDuoc,
            BestFitness = bestFitness,
            TheHeDaChay = generationsUsed,
            ThoiGianChayMs = stopwatch.Elapsed.TotalMilliseconds
        };
    }

    // ---- Data model ----

    private sealed class CandidateSlot
    {
        public int ThuTrongTuan;
        public int MaCaHoc;
        public int MaPhong;
        public int DayShift;     // (dayIdx * shiftCount) + shiftIdx
        public int RoomIdx;
        public int ShiftIdx;
        public double StaticScore;
    }

    private sealed class CourseDef
    {
        public int MaKhoaHoc { get; set; }
        public int MaMonHoc { get; set; }
        public int MaGiaoVien { get; set; }
        public string? TenGiaoVien { get; set; }
        public int TeacherIdx { get; set; }
        public int ClassIdx { get; set; }
        public int MaLop { get; set; }
        public int ExpectedStudentCount { get; set; }
        public int RequiredSlots { get; set; }
        public List<CandidateSlot> Feasible { get; set; } = new();
        public List<int> FeasibleByScore { get; set; } = new(); // indices into Feasible, desc static score

        public bool CanCoverRequiredSlots()
            => Feasible
                .Select(slot => slot.DayShift)
                .Distinct()
                .Count() >= RequiredSlots;
    }

    private sealed class TimetableProblem
    {
        public List<CourseDef> Courses { get; set; } = new();
        public List<int> UnassignableCourseIds { get; set; } = new();
        public List<Backend.Models.CaHoc> Shifts { get; set; } = new();
        public List<PhongHoc> Rooms { get; set; } = new();
        public int ShiftCount { get; set; }
        public int RoomCount { get; set; }
        public int MaxTeachers { get; set; }
        public int MaxClasses { get; set; }
    }

    private sealed class Chromosome
    {
        public int[][] Genes { get; set; } = Array.Empty<int[]>();
        public double Fitness { get; set; }

        public Chromosome Clone()
        {
            return new Chromosome
            {
                Genes = Genes.Select(g => g.ToArray()).ToArray(),
                Fitness = Fitness
            };
        }
    }

    private TimetableProblem BuildProblem(
        IReadOnlyList<KhoaHoc> courses,
        IReadOnlyList<Backend.Models.CaHoc> shifts,
        IReadOnlyList<PhongHoc> rooms,
        IReadOnlyDictionary<int, int> requiredSlots,
        IReadOnlyDictionary<int, int> studentCounts,
        IReadOnlyDictionary<int, IReadOnlySet<(int Day, int Shift)>> confirmedAvailabilityByTeacher)
    {
        var problem = new TimetableProblem
        {
            Shifts = shifts.ToList(),
            Rooms = rooms.ToList(),
            ShiftCount = shifts.Count,
            RoomCount = rooms.Count
        };

        var validCourses = courses.Where(c => c.MaGiaoVien > 0).ToList();
        var invalidCourses = courses.Where(c => c.MaGiaoVien <= 0).ToList();
        foreach (var inv in invalidCourses)
        {
            problem.UnassignableCourseIds.Add(inv.MaKhoaHoc);
        }

        var teacherIds = validCourses.Select(x => x.MaGiaoVien).Distinct().OrderBy(x => x).ToList();
        var teacherIndex = teacherIds.Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);
        var classIds = validCourses.Select(x => x.MaLop).Distinct().OrderBy(x => x).ToList();
        var classIndex = classIds.Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);
        problem.MaxTeachers = teacherIds.Count;
        problem.MaxClasses = classIds.Count;

        foreach (var course in validCourses)
        {
            var def = new CourseDef
            {
                MaKhoaHoc = course.MaKhoaHoc,
                MaMonHoc = course.MaMonHoc,
                MaGiaoVien = course.MaGiaoVien,
                TenGiaoVien = course.GiaoVien?.HoTen,
                TeacherIdx = teacherIndex[course.MaGiaoVien],
                MaLop = course.MaLop,
                ClassIdx = classIndex[course.MaLop],
                ExpectedStudentCount = studentCounts.GetValueOrDefault(course.MaLop, 0),
                RequiredSlots = requiredSlots.GetValueOrDefault(course.MaKhoaHoc, 1)
            };
            if (def.RequiredSlots <= 0) continue;

            var classSize = def.ExpectedStudentCount;
            var teacherId = course.MaGiaoVien;
            var hasConfirmedAvailability = confirmedAvailabilityByTeacher.TryGetValue(teacherId, out var availableSlots);

            for (var d = 0; d < WeekDays.Length; d++)
            {
                var day = WeekDays[d];
                for (var s = 0; s < shifts.Count; s++)
                {
                    var shift = shifts[s];
                    if (hasConfirmedAvailability && !availableSlots!.Contains((day, shift.MaCaHoc)))
                        continue;

                    for (var r = 0; r < rooms.Count; r++)
                    {
                        var room = rooms[r];
                        if (room.SucChua > 0 && classSize > 0 && room.SucChua < classSize) continue;

                        var slot = new CandidateSlot
                        {
                            ThuTrongTuan = day,
                            MaCaHoc = shift.MaCaHoc,
                            MaPhong = room.MaPhong,
                            DayShift = (d * shifts.Count) + s,
                            ShiftIdx = s,
                            RoomIdx = r
                        };
                        slot.StaticScore = ComputeStaticScore(slot, shift, room, classSize);
                        def.Feasible.Add(slot);
                    }
                }
            }

            def.FeasibleByScore = def.Feasible
                .Select((c, i) => (c, i))
                .OrderByDescending(x => x.c.StaticScore)
                .Select(x => x.i)
                .ToList();

            if (def.Feasible.Count > 0 && def.CanCoverRequiredSlots())
                problem.Courses.Add(def);
            else
                problem.UnassignableCourseIds.Add(course.MaKhoaHoc);
        }

        return problem;
    }

    private double ComputeStaticScore(CandidateSlot slot, Backend.Models.CaHoc shift, PhongHoc room, int classSize)
    {
        double score = _options.BaseScore;

        if (slot.ThuTrongTuan == 7) score -= _options.SaturdayPenalty;

        var isEvening = (shift.Buoi?.Contains("Tối", StringComparison.OrdinalIgnoreCase) == true) ||
                        (shift.TenCa?.Contains("Tối", StringComparison.OrdinalIgnoreCase) == true);
        if (isEvening) score -= _options.EveningPenalty;

        if (classSize > 0)
        {
            var ratio = room.SucChua > 0 ? (double)room.SucChua / classSize : 1.0;
            if (ratio >= 1.0 && ratio <= _options.OversizedRoomRatio) score += _options.GoodRoomFitBonus;
            else if (ratio > _options.OversizedRoomRatio) score -= _options.OversizedRoomPenalty;
        }

        return score;
    }

    // ---- Population ----

    private Chromosome CreateChromosome(TimetableProblem problem)
    {
        var chromo = new Chromosome
        {
            Genes = new int[problem.Courses.Count][]
        };
        for (var i = 0; i < problem.Courses.Count; i++)
        {
            chromo.Genes[i] = new int[problem.Courses[i].RequiredSlots];
        }
        return chromo;
    }

    private List<Chromosome> Initialize(TimetableProblem problem, int size)
    {
        var population = new List<Chromosome>(size);

        var greedy = CreateChromosome(problem);
        var occupied = new OccupancyState(problem.MaxTeachers, problem.MaxClasses, problem.RoomCount, problem.ShiftCount);

        var order = Enumerable.Range(0, problem.Courses.Count)
            .OrderBy(i => problem.Courses[i].Feasible.Count)
            .ToList();

        foreach (var idx in order)
        {
            var course = problem.Courses[idx];
            if (!course.CanCoverRequiredSlots())
            {
                for (var slotIdx = 0; slotIdx < course.RequiredSlots; slotIdx++)
                    greedy.Genes[idx][slotIdx] = -1;
                continue;
            }

            var teacherIdx = course.TeacherIdx;
            var placed = 0;
            var usedDays = new HashSet<int>();
            var usedShifts = new HashSet<int>();

            // Pass 1: distinct days
            foreach (var candIdx in course.FeasibleByScore)
            {
                if (placed >= course.RequiredSlots) break;
                var cand = course.Feasible[candIdx];
                if (!usedDays.Contains(cand.ThuTrongTuan) && occupied.IsFree(teacherIdx, course.ClassIdx, cand))
                {
                    greedy.Genes[idx][placed] = candIdx;
                    usedDays.Add(cand.ThuTrongTuan);
                    usedShifts.Add(cand.DayShift);
                    occupied.Occupy(teacherIdx, course.ClassIdx, cand);
                    placed++;
                }
            }

            // Pass 2: fallback if not enough distinct days
            if (placed < course.RequiredSlots)
            {
                foreach (var candIdx in course.FeasibleByScore)
                {
                    if (placed >= course.RequiredSlots) break;
                    var cand = course.Feasible[candIdx];
                    if (!usedShifts.Contains(cand.DayShift) && occupied.IsFree(teacherIdx, course.ClassIdx, cand))
                    {
                        greedy.Genes[idx][placed] = candIdx;
                        usedShifts.Add(cand.DayShift);
                        occupied.Occupy(teacherIdx, course.ClassIdx, cand);
                        placed++;
                    }
                }
            }

            for (; placed < course.RequiredSlots; placed++)
                greedy.Genes[idx][placed] = -1;
        }
        Evaluate(problem, greedy);
        population.Add(greedy);

        while (population.Count < size)
        {
            var chromo = CreateChromosome(problem);
            for (var i = 0; i < problem.Courses.Count; i++)
            {
                var course = problem.Courses[i];
                for (var slotIdx = 0; slotIdx < course.RequiredSlots; slotIdx++)
                    chromo.Genes[i][slotIdx] = _random.Next(course.Feasible.Count);
            }
            Evaluate(problem, chromo);
            population.Add(chromo);
        }

        return population;
    }

    private void Evaluate(TimetableProblem problem, Chromosome chromo)
    {
        var teacherOcc = new int[problem.MaxTeachers][];
        var classOcc = new int[problem.MaxClasses][];
        var roomOcc = new int[problem.RoomCount][];
        for (var i = 0; i < problem.MaxTeachers; i++) teacherOcc[i] = new int[problem.ShiftCount * 6];
        for (var i = 0; i < problem.MaxClasses; i++) classOcc[i] = new int[problem.ShiftCount * 6];
        for (var i = 0; i < problem.RoomCount; i++) roomOcc[i] = new int[problem.ShiftCount * 6];

        double fitness = 0;
        var conflicts = 0;

        for (var i = 0; i < problem.Courses.Count; i++)
        {
            var course = problem.Courses[i];
            var teacherIdx = course.TeacherIdx;

            for (var slotIdx = 0; slotIdx < course.RequiredSlots; slotIdx++)
            {
                var gene = chromo.Genes[i][slotIdx];
                if (gene < 0 || gene >= course.Feasible.Count)
                {
                    fitness -= _options.UnassignedSlotPenalty;
                    continue;
                }

                var cand = course.Feasible[gene];
                teacherOcc[teacherIdx][cand.DayShift]++;
                classOcc[course.ClassIdx][cand.DayShift]++;
                roomOcc[cand.RoomIdx][cand.DayShift]++;
                fitness += cand.StaticScore;
            }
        }

        // conflicts
        for (var i = 0; i < problem.Courses.Count; i++)
        {
            var course = problem.Courses[i];
            var teacherIdx = course.TeacherIdx;
            for (var slotIdx = 0; slotIdx < course.RequiredSlots; slotIdx++)
            {
                var gene = chromo.Genes[i][slotIdx];
                if (gene < 0 || gene >= course.Feasible.Count) continue;
                var cand = course.Feasible[gene];
                if (teacherOcc[teacherIdx][cand.DayShift] > 1) conflicts++;
                if (classOcc[course.ClassIdx][cand.DayShift] > 1) conflicts++;
                if (roomOcc[cand.RoomIdx][cand.DayShift] > 1) conflicts++;
            }
        }
        fitness -= conflicts * _options.HardConflictPenalty;

        // same-day duplicate + consecutive shift penalties per course
        for (var i = 0; i < problem.Courses.Count; i++)
        {
            var course = problem.Courses[i];
            var daySlots = new Dictionary<int, List<int>>();
            for (var slotIdx = 0; slotIdx < course.RequiredSlots; slotIdx++)
            {
                var gene = chromo.Genes[i][slotIdx];
                if (gene < 0 || gene >= course.Feasible.Count) continue;
                var cand = course.Feasible[gene];
                if (!daySlots.TryGetValue(cand.ThuTrongTuan, out var list))
                    daySlots[cand.ThuTrongTuan] = list = new List<int>();
                list.Add(cand.ShiftIdx);
            }

            foreach (var group in daySlots.Values)
            {
                if (group.Count > 1)
                    fitness -= _options.SameDayDuplicatePenalty * (group.Count - 1);

                var ordered = group.Distinct().OrderBy(x => x).ToList();
                for (var t = 0; t + 1 < ordered.Count; t++)
                {
                    if (ordered[t + 1] - ordered[t] == 1)
                        fitness -= _options.ConsecutiveShiftPenalty;
                }
            }
        }

        // teacher / class daily load penalties
        for (var i = 0; i < problem.Courses.Count; i++)
        {
            var course = problem.Courses[i];
            var teacherIdx = course.TeacherIdx;
            for (var slotIdx = 0; slotIdx < course.RequiredSlots; slotIdx++)
            {
                var gene = chromo.Genes[i][slotIdx];
                if (gene < 0 || gene >= course.Feasible.Count) continue;
                var cand = course.Feasible[gene];
                var dayOffset = cand.DayShift - cand.ShiftIdx;
                var teacherLoad = 0;
                var classLoad = 0;
                for (var s = 0; s < problem.ShiftCount; s++)
                {
                    teacherLoad += teacherOcc[teacherIdx][dayOffset + s];
                    classLoad += classOcc[course.ClassIdx][dayOffset + s];
                }
                if (teacherLoad >= _options.TeacherDailyLoadThreshold)
                    fitness -= _options.TeacherDailyLoadPenalty;
                if (classLoad >= _options.ClassDailyLoadThreshold)
                    fitness -= _options.ClassDailyLoadPenalty;
            }
        }

        // === SOFT CONSTRAINT MỚI 1: GV có khoảng trống 1 ca xen giữa 2 ca dạy cùng ngày ===
        // Phạt: GV bị xếp ca 1 và ca 3 cùng ngày (ca 2 trống) → di chuyển vô ích
        for (var t = 0; t < problem.MaxTeachers; t++)
        {
            for (var d = 0; d < 6; d++)
            {
                var dayOffset = d * problem.ShiftCount;
                var activeShifts = new List<int>(problem.ShiftCount);
                for (var s = 0; s < problem.ShiftCount; s++)
                {
                    if (teacherOcc[t][dayOffset + s] > 0)
                        activeShifts.Add(s);
                }

                if (activeShifts.Count >= 2)
                {
                    for (var i = 0; i < activeShifts.Count - 1; i++)
                    {
                        if (activeShifts[i + 1] - activeShifts[i] == 2)
                            fitness -= _options.TeacherDailyLoadPenalty; // Khoảng trống đúng 1 ca
                    }
                }
            }
        }

        // === SOFT CONSTRAINT MỚI 2: Lớp có khoảng trống > 1 ca giữa các môn cùng ngày ===
        // Phạt: Lớp học ca 1 và ca 4 cùng ngày (khoảng trống 2 ca) → chờ đợi lãng phí
        for (var c = 0; c < problem.MaxClasses; c++)
        {
            for (var d = 0; d < 6; d++)
            {
                var dayOffset = d * problem.ShiftCount;
                var activeShifts = new List<int>(problem.ShiftCount);
                for (var s = 0; s < problem.ShiftCount; s++)
                {
                    if (classOcc[c][dayOffset + s] > 0)
                        activeShifts.Add(s);
                }

                if (activeShifts.Count >= 2)
                {
                    for (var i = 0; i < activeShifts.Count - 1; i++)
                    {
                        if (activeShifts[i + 1] - activeShifts[i] > 2)
                            fitness -= _options.ClassDailyLoadPenalty; // Khoảng trống lớn hơn 1 ca
                    }
                }
            }
        }

        chromo.Fitness = fitness;
    }

    private Chromosome TournamentSelect(List<Chromosome> sorted, int k)
    {
        var best = sorted[_random.Next(sorted.Count)];
        for (var i = 1; i < k; i++)
        {
            var candidate = sorted[_random.Next(sorted.Count)];
            if (candidate.Fitness > best.Fitness) best = candidate;
        }
        return best;
    }

    private (Chromosome, Chromosome) Crossover(TimetableProblem problem, Chromosome a, Chromosome b, double probability)
    {
        var childA = CreateChromosome(problem);
        var childB = CreateChromosome(problem);
        for (var i = 0; i < problem.Courses.Count; i++)
        {
            var takeA = _random.NextDouble() < probability;
            childA.Genes[i] = takeA ? a.Genes[i].ToArray() : b.Genes[i].ToArray();
            childB.Genes[i] = takeA ? b.Genes[i].ToArray() : a.Genes[i].ToArray();
        }
        return (childA, childB);
    }

    private void Mutate(TimetableProblem problem, Chromosome chromo, double probability)
    {
        for (var i = 0; i < problem.Courses.Count; i++)
        {
            var course = problem.Courses[i];
            if (course.Feasible.Count == 0) continue;

            for (var slotIdx = 0; slotIdx < course.RequiredSlots; slotIdx++)
            {
                if (_random.NextDouble() < probability)
                    chromo.Genes[i][slotIdx] = _random.Next(course.Feasible.Count);
            }
        }
    }

    private int CountAssignedCourses(TimetableProblem problem, Chromosome chromo)
    {
        var count = 0;
        for (var i = 0; i < problem.Courses.Count; i++)
        {
            var course = problem.Courses[i];
            var full = true;
            for (var slotIdx = 0; slotIdx < course.RequiredSlots; slotIdx++)
            {
                var gene = chromo.Genes[i][slotIdx];
                if (gene < 0 || gene >= course.Feasible.Count) { full = false; break; }
            }
            if (full) count++;
        }
        return count;
    }

    // ---- Final repair ----

    private void RepairGreedy(TimetableProblem problem, Chromosome chromo)
    {
        var occupied = new OccupancyState(problem.MaxTeachers, problem.MaxClasses, problem.RoomCount, problem.ShiftCount);
        var preserved = new List<int>[problem.Courses.Count];

        // Step 1: Retain valid, conflict-free genes from the GA solution that have distinct days
        for (var i = 0; i < problem.Courses.Count; i++)
        {
            preserved[i] = new List<int>();
            var course = problem.Courses[i];
            var teacherIdx = course.TeacherIdx;
            var usedDays = new HashSet<int>();

            for (var slotIdx = 0; slotIdx < course.RequiredSlots; slotIdx++)
            {
                var gene = chromo.Genes[i][slotIdx];
                if (gene < 0 || gene >= course.Feasible.Count) continue;
                var cand = course.Feasible[gene];
                if (!usedDays.Contains(cand.ThuTrongTuan) && occupied.IsFree(teacherIdx, course.ClassIdx, cand))
                {
                    preserved[i].Add(gene);
                    usedDays.Add(cand.ThuTrongTuan);
                    occupied.Occupy(teacherIdx, course.ClassIdx, cand);
                }
            }
        }

        // Step 2: Complete any missing slots for each course, preferring distinct days first
        var order = Enumerable.Range(0, problem.Courses.Count)
            .OrderBy(i => problem.Courses[i].Feasible.Count)
            .ThenByDescending(i => problem.Courses[i].RequiredSlots)
            .ThenBy(i => problem.Courses[i].MaKhoaHoc)
            .ToList();

        foreach (var courseIdx in order)
        {
            var course = problem.Courses[courseIdx];
            var teacherIdx = course.TeacherIdx;
            var currentGenes = preserved[courseIdx];
            var usedDays = new HashSet<int>(currentGenes.Select(g => course.Feasible[g].ThuTrongTuan));
            var usedShifts = new HashSet<int>(currentGenes.Select(g => course.Feasible[g].DayShift));

            // Pass 1: Find free slots on unused days
            if (currentGenes.Count < course.RequiredSlots)
            {
                foreach (var candidateIndex in course.FeasibleByScore)
                {
                    if (currentGenes.Count == course.RequiredSlots) break;
                    var candidate = course.Feasible[candidateIndex];
                    if (usedDays.Contains(candidate.ThuTrongTuan)) continue;
                    if (!occupied.IsFree(teacherIdx, course.ClassIdx, candidate)) continue;

                    currentGenes.Add(candidateIndex);
                    usedDays.Add(candidate.ThuTrongTuan);
                    usedShifts.Add(candidate.DayShift);
                    occupied.Occupy(teacherIdx, course.ClassIdx, candidate);
                }
            }

            // Pass 2: Fallback to sharing days if necessary
            if (currentGenes.Count < course.RequiredSlots)
            {
                foreach (var candidateIndex in course.FeasibleByScore)
                {
                    if (currentGenes.Count == course.RequiredSlots) break;
                    var candidate = course.Feasible[candidateIndex];
                    if (usedShifts.Contains(candidate.DayShift)) continue;
                    if (!occupied.IsFree(teacherIdx, course.ClassIdx, candidate)) continue;

                    currentGenes.Add(candidateIndex);
                    usedShifts.Add(candidate.DayShift);
                    occupied.Occupy(teacherIdx, course.ClassIdx, candidate);
                }
            }

            if (currentGenes.Count == course.RequiredSlots)
            {
                for (var s = 0; s < course.RequiredSlots; s++)
                    chromo.Genes[courseIdx][s] = currentGenes[s];
            }
            else
            {
                foreach (var g in currentGenes)
                    occupied.Release(teacherIdx, course.ClassIdx, course.Feasible[g]);
                for (var s = 0; s < course.RequiredSlots; s++)
                    chromo.Genes[courseIdx][s] = -1;
            }
        }
    }

    // ---- Decode ----

    private (List<TimetableAssignment>, int, int) Decode(TimetableProblem problem, Chromosome chromo)
    {
        var assignments = new List<TimetableAssignment>();
        var xepDuoc = 0;
        var khongXepDuoc = 0;

        for (var i = 0; i < problem.Courses.Count; i++)
        {
            var course = problem.Courses[i];
            var full = true;
            var courseAssignments = new List<TimetableAssignment>();
            var dayShifts = new HashSet<int>();
            var conflicts = 0;

            var maGiaoVien = course.MaGiaoVien;
            var tenGiaoVien = course.TenGiaoVien;
            var mucDoPhuHop = (int?)null; // Không còn tính toán độ phù hợp GV trong GA vì GV đã cố định theo phân công khóa học

            var teacherDayLoads = new Dictionary<int, int>();
            var classDayLoads = new Dictionary<int, int>();

            // count loads per day
            for (var slotIdx = 0; slotIdx < course.RequiredSlots; slotIdx++)
            {
                var gene = chromo.Genes[i][slotIdx];
                if (gene < 0 || gene >= course.Feasible.Count) { full = false; continue; }
                var cand = course.Feasible[gene];
                var dayOffset = cand.DayShift - cand.ShiftIdx;
                teacherDayLoads.TryGetValue(dayOffset, out var tl);
                classDayLoads.TryGetValue(dayOffset, out var cl);
                teacherDayLoads[dayOffset] = tl + 1;
                classDayLoads[dayOffset] = cl + 1;
            }

            var teacherReason = tenGiaoVien != null
                ? $"Giảng viên {tenGiaoVien} phụ trách khóa học."
                : $"Giảng viên mã {maGiaoVien} phụ trách khóa học.";

            for (var slotIdx = 0; slotIdx < course.RequiredSlots; slotIdx++)
            {
                var gene = chromo.Genes[i][slotIdx];
                if (gene < 0 || gene >= course.Feasible.Count) continue;
                var cand = course.Feasible[gene];
                var dayOffset = cand.DayShift - cand.ShiftIdx;

                var context = new ScheduleCandidateContext
                {
                    Course = new KhoaHoc { MaKhoaHoc = course.MaKhoaHoc, MaGiaoVien = maGiaoVien, MaLop = course.MaLop },
                    Shift = problem.Shifts[cand.ShiftIdx],
                    Room = problem.Rooms[cand.RoomIdx],
                    DayOfWeek = cand.ThuTrongTuan,
                    PreferenceLevel = null,
                    TeacherDailyLoad = teacherDayLoads.GetValueOrDefault(dayOffset) - 1,
                    ClassDailyLoad = classDayLoads.GetValueOrDefault(dayOffset) - 1,
                    ExpectedStudentCount = course.ExpectedStudentCount
                };

                var suggestion = _scoringService.ScoreCandidate(context);

                if (!dayShifts.Add(cand.DayShift)) conflicts++;

                courseAssignments.Add(new TimetableAssignment
                {
                    MaKhoaHoc = course.MaKhoaHoc,
                    MaGiaoVien = maGiaoVien,
                    TenGiaoVien = tenGiaoVien,
                    MucDoPhuHop = mucDoPhuHop,
                    ThuTrongTuan = cand.ThuTrongTuan,
                    MaCaHoc = cand.MaCaHoc,
                    MaPhong = cand.MaPhong,
                    Score = suggestion.Score,
                    Components = suggestion.Components,
                    Reasons = new List<string> { teacherReason }.Concat(suggestion.Reasons).ToList(),
                    Warnings = suggestion.Warnings
                });
            }

            if (full)
            {
                xepDuoc++;
                assignments.AddRange(courseAssignments);
            }
            else
            {
                khongXepDuoc++;
            }
        }

        return (assignments, xepDuoc, khongXepDuoc + problem.UnassignableCourseIds.Count);
    }

    // ---- Occupancy helper ----

    private sealed class OccupancyState
    {
        private readonly int[][] _teacher;
        private readonly int[][] _class;
        private readonly int[][] _room;
        private readonly int _shiftCount;

        public OccupancyState(int teachers, int classes, int rooms, int shiftCount)
        {
            _shiftCount = shiftCount;
            var slots = shiftCount * 6;
            _teacher = Create(teachers, slots);
            _class = Create(classes, slots);
            _room = Create(rooms, slots);
        }

        private static int[][] Create(int n, int slots)
        {
            var arr = new int[n][];
            for (var i = 0; i < n; i++) arr[i] = new int[slots];
            return arr;
        }

        public bool IsFree(int teacherIdx, int classIdx, CandidateSlot cand)
            => _teacher[teacherIdx][cand.DayShift] == 0 &&
               _class[classIdx][cand.DayShift] == 0 &&
               _room[cand.RoomIdx][cand.DayShift] == 0;

        public void Occupy(int teacherIdx, int classIdx, CandidateSlot cand)
        {
            _teacher[teacherIdx][cand.DayShift]++;
            _class[classIdx][cand.DayShift]++;
            _room[cand.RoomIdx][cand.DayShift]++;
        }

        public void Release(int teacherIdx, int classIdx, CandidateSlot cand)
        {
            if (_teacher[teacherIdx][cand.DayShift] > 0) _teacher[teacherIdx][cand.DayShift]--;
            if (_class[classIdx][cand.DayShift] > 0) _class[classIdx][cand.DayShift]--;
            if (_room[cand.RoomIdx][cand.DayShift] > 0) _room[cand.RoomIdx][cand.DayShift]--;
        }
    }
}
