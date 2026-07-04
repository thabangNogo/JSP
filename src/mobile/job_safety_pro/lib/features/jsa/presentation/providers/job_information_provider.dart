import 'package:flutter_riverpod/flutter_riverpod.dart';

class JobInformationFormState {
  const JobInformationFormState({
    this.workLocationId,
    this.workSectionId,
    this.showValidation = false,
    this.isHydrated = false,
  });

  final String? workLocationId;
  final String? workSectionId;
  final bool showValidation;
  final bool isHydrated;

  JobInformationFormState copyWith({
    String? workLocationId,
    String? workSectionId,
    bool? showValidation,
    bool? isHydrated,
  }) {
    return JobInformationFormState(
      workLocationId: workLocationId ?? this.workLocationId,
      workSectionId: workSectionId ?? this.workSectionId,
      showValidation: showValidation ?? this.showValidation,
      isHydrated: isHydrated ?? this.isHydrated,
    );
  }
}

class JobInformationNotifier extends Notifier<JobInformationFormState> {
  @override
  JobInformationFormState build() => const JobInformationFormState();

  void hydrateFromDraft({
    String? workLocationId,
    String? workSectionId,
    bool force = false,
  }) {
    if (!force && state.isHydrated) return;
    state = JobInformationFormState(
      workLocationId: workLocationId,
      workSectionId: workSectionId,
      isHydrated: true,
      showValidation: false,
    );
  }

  void setWorkLocationId(String? value) => state = state.copyWith(workLocationId: value);
  void setWorkSectionId(String? value) => state = state.copyWith(workSectionId: value);

  bool validate() {
    final valid = state.workLocationId != null &&
        state.workLocationId!.isNotEmpty &&
        state.workSectionId != null &&
        state.workSectionId!.isNotEmpty;
    state = state.copyWith(showValidation: !valid);
    return valid;
  }
}

final jobInformationProvider =
    NotifierProvider<JobInformationNotifier, JobInformationFormState>(
  JobInformationNotifier.new,
);
