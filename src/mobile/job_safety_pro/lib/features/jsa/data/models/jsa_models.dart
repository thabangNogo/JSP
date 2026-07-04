import '../../../../shared/enums/assessment_status.dart';
import '../../domain/models/quick_assessment_models.dart';

String _readString(dynamic value) => value?.toString() ?? '';

class HazardDraftModel {
  HazardDraftModel({
    required this.id,
    required this.description,
    this.catalogHazardId,
    this.consequence = '',
    this.selectedControlIds = const [],
    this.riskLevelId,
    this.residualRiskLevelId,
    this.photoPaths = const [],
  });

  factory HazardDraftModel.fromJson(Map<String, dynamic> json) => HazardDraftModel(
        id: json['id'] as String,
        description: json['description'] as String,
        catalogHazardId: json['catalogHazardId'] as String?,
        consequence: json['consequence'] as String? ?? '',
        selectedControlIds:
            (json['selectedControlIds'] as List<dynamic>? ?? []).cast<String>(),
        riskLevelId: json['riskLevelId'] as String?,
        residualRiskLevelId: json['residualRiskLevelId'] as String?,
        photoPaths: (json['photoPaths'] as List<dynamic>? ?? []).cast<String>(),
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'description': description,
        if (catalogHazardId != null) 'catalogHazardId': catalogHazardId,
        if (consequence.isNotEmpty) 'consequence': consequence,
        if (selectedControlIds.isNotEmpty) 'selectedControlIds': selectedControlIds,
        if (riskLevelId != null) 'riskLevelId': riskLevelId,
        if (residualRiskLevelId != null) 'residualRiskLevelId': residualRiskLevelId,
        'photoPaths': photoPaths,
      };

  HazardDraftModel copyWith({
    String? id,
    String? description,
    String? catalogHazardId,
    String? consequence,
    List<String>? selectedControlIds,
    String? riskLevelId,
    String? residualRiskLevelId,
    List<String>? photoPaths,
    bool clearRiskLevelId = false,
    bool clearResidualRiskLevelId = false,
  }) {
    return HazardDraftModel(
      id: id ?? this.id,
      description: description ?? this.description,
      catalogHazardId: catalogHazardId ?? this.catalogHazardId,
      consequence: consequence ?? this.consequence,
      selectedControlIds: selectedControlIds ?? this.selectedControlIds,
      riskLevelId: clearRiskLevelId ? null : (riskLevelId ?? this.riskLevelId),
      residualRiskLevelId:
          clearResidualRiskLevelId ? null : (residualRiskLevelId ?? this.residualRiskLevelId),
      photoPaths: photoPaths ?? this.photoPaths,
    );
  }

  final String id;
  final String description;
  final String? catalogHazardId;
  final String consequence;
  final List<String> selectedControlIds;
  final String? riskLevelId;
  final String? residualRiskLevelId;
  final List<String> photoPaths;
}

class ControlDraftModel {
  ControlDraftModel({
    required this.id,
    required this.description,
    required this.hierarchyOfControl,
    this.catalogControlId,
    this.hazardId,
    this.isImplemented = false,
  });

  factory ControlDraftModel.fromJson(Map<String, dynamic> json) => ControlDraftModel(
        id: json['id'] as String,
        description: json['description'] as String,
        hierarchyOfControl: json['hierarchyOfControl'] as String,
        catalogControlId: json['catalogControlId'] as String?,
        hazardId: json['hazardId'] as String?,
        isImplemented: json['isImplemented'] as bool? ?? false,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'description': description,
        'hierarchyOfControl': hierarchyOfControl,
        if (catalogControlId != null) 'catalogControlId': catalogControlId,
        if (hazardId != null) 'hazardId': hazardId,
        'isImplemented': isImplemented,
      };

  final String id;
  final String description;
  final String hierarchyOfControl;
  final String? catalogControlId;
  final String? hazardId;
  final bool isImplemented;
}

class AssessmentDraftModel {
  AssessmentDraftModel({
    required this.localId,
    this.remoteId,
    required this.title,
    required this.jobDescription,
    required this.companyId,
    required this.plantId,
    required this.departmentId,
    required this.currentStep,
    this.department = '',
    this.location = '',
    this.section = '',
    this.workLocationId,
    this.workSectionId,
    this.observeStopNotes = '',
    this.conversationNotes = '',
    this.quickAssessment = const QuickAssessmentModel(),
    this.hazards = const [],
    this.controls = const [],
    this.signaturePath,
    this.qrScannedCode,
    this.updatedAt,
    this.isSynced = false,
    this.status = AssessmentStatus.draft,
    this.signOffName,
    this.signOffSurname,
    this.signOffCompanyNumber,
    this.signOffOccupation,
  });

  factory AssessmentDraftModel.fromJson(Map<String, dynamic> json) => AssessmentDraftModel(
        localId: json['localId'] as String,
        remoteId: json['remoteId'] as String?,
        title: json['title'] as String,
        jobDescription: json['jobDescription'] as String,
        companyId: json['companyId'] as String,
        plantId: json['plantId'] as String,
        departmentId: json['departmentId'] as String,
        currentStep: json['currentStep'] as int? ?? 0,
        department: json['department'] as String? ?? '',
        location: json['location'] as String? ?? '',
        section: json['section'] as String? ?? '',
        workLocationId: json['workLocationId']?.toString(),
        workSectionId: json['workSectionId']?.toString(),
        observeStopNotes: json['observeStopNotes'] as String? ?? '',
        conversationNotes: json['conversationNotes'] as String? ?? '',
        quickAssessment: json['quickAssessment'] != null
            ? QuickAssessmentModel.fromJson(
                Map<String, dynamic>.from(json['quickAssessment'] as Map),
              )
            : const QuickAssessmentModel(),
        hazards: (json['hazards'] as List<dynamic>? ?? [])
            .map((e) => HazardDraftModel.fromJson(Map<String, dynamic>.from(e as Map)))
            .toList(),
        controls: (json['controls'] as List<dynamic>? ?? [])
            .map((e) => ControlDraftModel.fromJson(Map<String, dynamic>.from(e as Map)))
            .toList(),
        signaturePath: json['signaturePath'] as String?,
        qrScannedCode: json['qrScannedCode'] as String?,
        updatedAt: json['updatedAt'] != null
            ? DateTime.parse(json['updatedAt'] as String)
            : null,
        isSynced: json['isSynced'] as bool? ?? false,
        status: AssessmentStatus.parse(json['status'] ?? json['statusName']),
        signOffName: json['signOffName'] as String?,
        signOffSurname: json['signOffSurname'] as String?,
        signOffCompanyNumber: json['signOffCompanyNumber'] as String?,
        signOffOccupation: json['signOffOccupation'] as String?,
      );

  Map<String, dynamic> toJson() => {
        'localId': localId,
        'remoteId': remoteId,
        'title': title,
        'jobDescription': jobDescription,
        'companyId': companyId,
        'plantId': plantId,
        'departmentId': departmentId,
        'currentStep': currentStep,
        'department': department,
        'location': location,
        'section': section,
        if (workLocationId != null) 'workLocationId': workLocationId,
        if (workSectionId != null) 'workSectionId': workSectionId,
        'observeStopNotes': observeStopNotes,
        'conversationNotes': conversationNotes,
        'quickAssessment': quickAssessment.toJson(),
        'hazards': hazards.map((e) => e.toJson()).toList(),
        'controls': controls.map((e) => e.toJson()).toList(),
        'signaturePath': signaturePath,
        'qrScannedCode': qrScannedCode,
        'updatedAt': (updatedAt ?? DateTime.now()).toIso8601String(),
        'isSynced': isSynced,
        'status': status.value,
        'statusName': status.name,
        if (signOffName != null) 'signOffName': signOffName,
        if (signOffSurname != null) 'signOffSurname': signOffSurname,
        if (signOffCompanyNumber != null) 'signOffCompanyNumber': signOffCompanyNumber,
        if (signOffOccupation != null) 'signOffOccupation': signOffOccupation,
      };

  AssessmentDraftModel copyWith({
    String? title,
    String? jobDescription,
    String? companyId,
    String? plantId,
    String? departmentId,
    int? currentStep,
    String? department,
    String? location,
    String? section,
    String? workLocationId,
    String? workSectionId,
    String? observeStopNotes,
    String? conversationNotes,
    QuickAssessmentModel? quickAssessment,
    List<HazardDraftModel>? hazards,
    List<ControlDraftModel>? controls,
    String? signaturePath,
    String? qrScannedCode,
    String? remoteId,
    bool? isSynced,
    AssessmentStatus? status,
    String? signOffName,
    String? signOffSurname,
    String? signOffCompanyNumber,
    String? signOffOccupation,
    DateTime? updatedAt,
  }) {
    return AssessmentDraftModel(
      localId: localId,
      remoteId: remoteId ?? this.remoteId,
      title: title ?? this.title,
      jobDescription: jobDescription ?? this.jobDescription,
      companyId: companyId ?? this.companyId,
      plantId: plantId ?? this.plantId,
      departmentId: departmentId ?? this.departmentId,
      currentStep: currentStep ?? this.currentStep,
      department: department ?? this.department,
      location: location ?? this.location,
      section: section ?? this.section,
      workLocationId: workLocationId ?? this.workLocationId,
      workSectionId: workSectionId ?? this.workSectionId,
      observeStopNotes: observeStopNotes ?? this.observeStopNotes,
      conversationNotes: conversationNotes ?? this.conversationNotes,
      quickAssessment: quickAssessment ?? this.quickAssessment,
      hazards: hazards ?? this.hazards,
      controls: controls ?? this.controls,
      signaturePath: signaturePath ?? this.signaturePath,
      qrScannedCode: qrScannedCode ?? this.qrScannedCode,
      updatedAt: updatedAt ?? this.updatedAt ?? DateTime.now(),
      isSynced: isSynced ?? this.isSynced,
      status: status ?? this.status,
      signOffName: signOffName ?? this.signOffName,
      signOffSurname: signOffSurname ?? this.signOffSurname,
      signOffCompanyNumber: signOffCompanyNumber ?? this.signOffCompanyNumber,
      signOffOccupation: signOffOccupation ?? this.signOffOccupation,
    );
  }

  final String localId;
  final String? remoteId;
  final String title;
  final String jobDescription;
  final String companyId;
  final String plantId;
  final String departmentId;
  final int currentStep;
  final String department;
  final String location;
  final String section;
  final String? workLocationId;
  final String? workSectionId;
  final String observeStopNotes;
  final String conversationNotes;
  final QuickAssessmentModel quickAssessment;
  final List<HazardDraftModel> hazards;
  final List<ControlDraftModel> controls;
  final String? signaturePath;
  final String? qrScannedCode;
  final DateTime? updatedAt;
  final bool isSynced;
  final AssessmentStatus status;
  final String? signOffName;
  final String? signOffSurname;
  final String? signOffCompanyNumber;
  final String? signOffOccupation;
}

class JsaModel {
  JsaModel({
    required this.id,
    required this.title,
    required this.jobDescription,
    required this.status,
    required this.plantId,
    required this.departmentId,
  });

  factory JsaModel.fromJson(Map<String, dynamic> json) => JsaModel(
        id: json['id']?.toString() ?? '',
        title: json['title'] as String,
        jobDescription: json['jobDescription'] as String,
        status: _readString(json['status']),
        plantId: json['plantId']?.toString() ?? '',
        departmentId: json['departmentId']?.toString() ?? '',
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'title': title,
        'jobDescription': jobDescription,
        'status': status,
        'plantId': plantId,
        'departmentId': departmentId,
      };

  final String id;
  final String title;
  final String jobDescription;
  final String status;
  final String plantId;
  final String departmentId;
}

class RiskLevelModel {
  RiskLevelModel({
    required this.id,
    required this.code,
    required this.name,
    required this.numericValue,
    required this.colorHex,
  });

  factory RiskLevelModel.fromJson(Map<String, dynamic> json) => RiskLevelModel(
        id: json['id']?.toString() ?? '',
        code: json['code'] as String,
        name: json['name'] as String,
        numericValue: (json['numericValue'] as num).toInt(),
        colorHex: json['colorHex'] as String,
      );

  final String id;
  final String code;
  final String name;
  final int numericValue;
  final String colorHex;
}

class HazardDetailItem {
  const HazardDetailItem({
    required this.description,
    this.consequence = '',
    this.riskLevelId,
    this.residualRiskLevelId,
  });

  final String description;
  final String consequence;
  final String? riskLevelId;
  final String? residualRiskLevelId;
}

class ControlDetailItem {
  const ControlDetailItem({
    required this.description,
    required this.hierarchyOfControl,
    this.isImplemented = false,
    this.hazardId,
  });

  final String description;
  final String hierarchyOfControl;
  final bool isImplemented;
  final String? hazardId;
}

class AssessmentDetailModel {
  const AssessmentDetailModel({
    required this.id,
    required this.localId,
    required this.title,
    required this.jobDescription,
    required this.status,
    required this.hazards,
    required this.controls,
    this.quickAssessmentSummary,
    this.observeStopNotes = '',
    this.conversationNotes = '',
    this.lastUpdated,
    this.signOffName,
    this.signOffSurname,
    this.signOffCompanyNumber,
    this.signOffOccupation,
    this.signaturePath,
    this.currentStep = 0,
    this.department = '',
    this.location = '',
    this.section = '',
  });

  final String id;
  final String localId;
  final String title;
  final String jobDescription;
  final String? quickAssessmentSummary;
  final String observeStopNotes;
  final String conversationNotes;
  final AssessmentStatus status;
  final DateTime? lastUpdated;
  final String? signOffName;
  final String? signOffSurname;
  final String? signOffCompanyNumber;
  final String? signOffOccupation;
  final String? signaturePath;
  final List<HazardDetailItem> hazards;
  final List<ControlDetailItem> controls;
  final int currentStep;
  final String department;
  final String location;
  final String section;

  bool get canContinueEditing => status.isDraft;

  String get signOffFullName {
    final parts = [signOffName, signOffSurname].where((p) => p != null && p.trim().isNotEmpty);
    return parts.join(' ').trim();
  }
}
