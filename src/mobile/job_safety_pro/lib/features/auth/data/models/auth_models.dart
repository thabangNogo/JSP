class UserModel {
  UserModel({
    required this.id,
    required this.email,
    required this.firstName,
    required this.lastName,
    required this.companyId,
    this.plantId,
    this.departmentId,
    required this.roles,
    this.employeeNumber = '',
  });

  factory UserModel.fromJson(Map<String, dynamic> json) => UserModel(
        id: json['id'].toString(),
        email: json['email'] as String,
        firstName: json['firstName'] as String,
        lastName: json['lastName'] as String,
        companyId: json['companyId'].toString(),
        plantId: json['plantId']?.toString(),
        departmentId: json['departmentId']?.toString(),
        roles: (json['roles'] as List<dynamic>).cast<String>(),
        employeeNumber: json['employeeNumber'] as String? ?? '',
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'email': email,
        'firstName': firstName,
        'lastName': lastName,
        'companyId': companyId,
        'plantId': plantId,
        'departmentId': departmentId,
        'roles': roles,
        'employeeNumber': employeeNumber,
      };

  final String id;
  final String email;
  final String firstName;
  final String lastName;
  final String companyId;
  final String? plantId;
  final String? departmentId;
  final List<String> roles;
  final String employeeNumber;

  String get fullName => '$firstName $lastName';
}

class AuthResponseModel {
  AuthResponseModel({
    required this.accessToken,
    required this.refreshToken,
    required this.accessTokenExpiresAt,
    required this.user,
  });

  factory AuthResponseModel.fromJson(Map<String, dynamic> json) =>
      AuthResponseModel(
        accessToken: json['accessToken'] as String,
        refreshToken: json['refreshToken'] as String,
        accessTokenExpiresAt: DateTime.parse(json['accessTokenExpiresAt'] as String),
        user: UserModel.fromJson(json['user'] as Map<String, dynamic>),
      );

  final String accessToken;
  final String refreshToken;
  final DateTime accessTokenExpiresAt;
  final UserModel user;
}

class LoginRequestModel {
  LoginRequestModel({required this.email, required this.password});

  Map<String, dynamic> toJson() => {'email': email, 'password': password};

  final String email;
  final String password;
}
