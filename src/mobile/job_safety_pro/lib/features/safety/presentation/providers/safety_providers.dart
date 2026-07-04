import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/dio_provider.dart';
import '../../data/datasources/safety_remote_datasource.dart';

final safetyRemoteDataSourceProvider = Provider(
  (ref) => SafetyRemoteDataSource(ref.watch(dioProvider)),
);

class EmployeeKpiState {
  const EmployeeKpiState({
    this.nearMissesThisMonth = 0,
    this.draftAssessments = 0,
    this.submittedAssessments = 0,
    this.approvedAssessments = 0,
    this.participationScore = 0,
    this.isLoading = false,
  });

  final int nearMissesThisMonth;
  final int draftAssessments;
  final int submittedAssessments;
  final int approvedAssessments;
  final int participationScore;
  final bool isLoading;

  EmployeeKpiState copyWith({
    int? nearMissesThisMonth,
    int? draftAssessments,
    int? submittedAssessments,
    int? approvedAssessments,
    int? participationScore,
    bool? isLoading,
  }) {
    return EmployeeKpiState(
      nearMissesThisMonth: nearMissesThisMonth ?? this.nearMissesThisMonth,
      draftAssessments: draftAssessments ?? this.draftAssessments,
      submittedAssessments: submittedAssessments ?? this.submittedAssessments,
      approvedAssessments: approvedAssessments ?? this.approvedAssessments,
      participationScore: participationScore ?? this.participationScore,
      isLoading: isLoading ?? this.isLoading,
    );
  }
}

class EmployeeKpiNotifier extends Notifier<EmployeeKpiState> {
  @override
  EmployeeKpiState build() {
    Future.microtask(load);
    return const EmployeeKpiState(isLoading: true);
  }

  Future<void> load() async {
    state = state.copyWith(isLoading: true);
    try {
      final json = await ref.read(safetyRemoteDataSourceProvider).getEmployeeKpis();
      state = EmployeeKpiState(
        nearMissesThisMonth: json['nearMissesSubmittedThisMonth'] as int? ?? 0,
        draftAssessments: json['myDraftAssessments'] as int? ?? 0,
        submittedAssessments: json['mySubmittedAssessments'] as int? ?? 0,
        approvedAssessments: json['myApprovedAssessments'] as int? ?? 0,
        participationScore: json['safetyParticipationScore'] as int? ?? 0,
      );
    } catch (e) {
      // Keep prior counts on refresh failure; avoid flashing zeros after a successful load.
      state = state.copyWith(isLoading: false);
      assert(() {
        // ignore: avoid_print
        print('Employee KPI load failed: $e');
        return true;
      }());
    }
  }
}

final employeeKpiProvider =
    NotifierProvider<EmployeeKpiNotifier, EmployeeKpiState>(EmployeeKpiNotifier.new);

class InjuryFreeDaysNotifier extends AsyncNotifier<int> {
  @override
  Future<int> build() async {
    return _load();
  }

  Future<int> load() async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(_load);
    return state.value ?? 0;
  }

  Future<int> _load() async {
    try {
      return await ref.read(safetyRemoteDataSourceProvider).getInjuryFreeDays();
    } catch (_) {
      final employeeKpis = await ref.read(safetyRemoteDataSourceProvider).getEmployeeKpis();
      return employeeKpis['injuryFreeDays'] as int? ?? 0;
    }
  }
}

final injuryFreeDaysProvider = AsyncNotifierProvider<InjuryFreeDaysNotifier, int>(
  InjuryFreeDaysNotifier.new,
);
