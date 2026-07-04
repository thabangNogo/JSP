class ApiResponse<T> {
  ApiResponse({
    required this.success,
    this.data,
    this.message,
  });

  factory ApiResponse.fromJson(
    Map<String, dynamic> json,
    T Function(dynamic json) fromJsonT,
  ) {
    return ApiResponse(
      success: json['success'] as bool? ?? true,
      data: json['data'] != null ? fromJsonT(json['data']) : null,
      message: json['message'] as String?,
    );
  }

  final bool success;
  final T? data;
  final String? message;
}
