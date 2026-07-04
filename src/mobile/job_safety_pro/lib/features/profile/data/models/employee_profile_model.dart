import '../../../jsa/data/models/work_lookup_models.dart';

class EmployeeProfileModel {
  const EmployeeProfileModel({
    required this.name,
    required this.surname,
    required this.companyNumber,
    required this.occupation,
    this.workDepartmentId,
    this.workDepartmentName = '',
    this.id,
    this.userId,
    this.updatedAt,
  });

  factory EmployeeProfileModel.fromJson(Map<String, dynamic> json) => EmployeeProfileModel(
        id: json['id']?.toString(),
        userId: json['userId']?.toString(),
        workDepartmentId:
            json['workDepartmentId']?.toString() ?? json['WorkDepartmentId']?.toString(),
        workDepartmentName: json['workDepartmentName'] as String? ??
            json['WorkDepartmentName'] as String? ??
            '',
        name: json['name'] as String? ?? '',
        surname: json['surname'] as String? ?? '',
        companyNumber: json['companyNumber'] as String? ?? '',
        occupation: json['occupation'] as String? ?? '',
        updatedAt: json['updatedAt'] != null
            ? DateTime.tryParse(json['updatedAt'] as String)
            : null,
      );

  final String? id;
  final String? userId;
  final String? workDepartmentId;
  final String workDepartmentName;
  final String name;
  final String surname;
  final String companyNumber;
  final String occupation;
  final DateTime? updatedAt;

  bool get isComplete =>
      name.trim().isNotEmpty &&
      surname.trim().isNotEmpty &&
      companyNumber.trim().isNotEmpty &&
      occupation.trim().isNotEmpty &&
      workDepartmentId != null &&
      workDepartmentId!.isNotEmpty;

  String get fullName => '${name.trim()} ${surname.trim()}'.trim();

  /// Uses cached name from API, or resolves from master data when only the id is known.
  String resolveDepartmentName(List<WorkLookupItem> departments) {
    if (workDepartmentName.trim().isNotEmpty) return workDepartmentName;
    if (workDepartmentId == null || workDepartmentId!.isEmpty) return '';
    for (final item in departments) {
      if (item.id == workDepartmentId) return item.name;
    }
    return '';
  }

  Map<String, dynamic> toJson() => {
        if (id != null) 'id': id,
        if (userId != null) 'userId': userId,
        if (workDepartmentId != null) 'workDepartmentId': workDepartmentId,
        'workDepartmentName': workDepartmentName,
        'name': name,
        'surname': surname,
        'companyNumber': companyNumber,
        'occupation': occupation,
        if (updatedAt != null) 'updatedAt': updatedAt!.toIso8601String(),
      };

  EmployeeProfileModel copyWith({
    String? name,
    String? surname,
    String? companyNumber,
    String? occupation,
    String? workDepartmentId,
    String? workDepartmentName,
    DateTime? updatedAt,
  }) {
    return EmployeeProfileModel(
      id: id,
      userId: userId,
      workDepartmentId: workDepartmentId ?? this.workDepartmentId,
      workDepartmentName: workDepartmentName ?? this.workDepartmentName,
      name: name ?? this.name,
      surname: surname ?? this.surname,
      companyNumber: companyNumber ?? this.companyNumber,
      occupation: occupation ?? this.occupation,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }
}
