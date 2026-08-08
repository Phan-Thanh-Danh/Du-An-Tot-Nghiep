import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

const String _configuredApiBaseUrl = String.fromEnvironment('API_BASE_URL');

String get apiBaseUrl {
  if (_configuredApiBaseUrl.isNotEmpty) return _configuredApiBaseUrl;

  if (kIsWeb ||
      defaultTargetPlatform == TargetPlatform.windows ||
      defaultTargetPlatform == TargetPlatform.macOS ||
      defaultTargetPlatform == TargetPlatform.linux ||
      defaultTargetPlatform == TargetPlatform.iOS) {
    return 'http://127.0.0.1:5097/api';
  }

  return 'http://10.0.2.2:5097/api';
}

class ApiClientException implements Exception {
  final String message;
  final int? statusCode;

  const ApiClientException(this.message, {this.statusCode});

  @override
  String toString() => message;
}

class ApiUnavailableException extends ApiClientException {
  const ApiUnavailableException(super.message);
}

final secureStorageProvider = Provider<FlutterSecureStorage>((ref) {
  return const FlutterSecureStorage();
});

final dioProvider = Provider<Dio>((ref) {
  final dio = Dio(
    BaseOptions(
      baseUrl: apiBaseUrl,
      connectTimeout: const Duration(seconds: 7),
      receiveTimeout: const Duration(seconds: 30),
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
    ),
  );

  final storage = ref.watch(secureStorageProvider);

  dio.interceptors.add(
    InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await storage.read(key: 'auth_token');
        if (token != null) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        return handler.next(options);
      },
      onError: (e, handler) {
        final data = e.response?.data;
        String message = 'Không thể kết nối đến máy chủ. Vui lòng thử lại.';
        if (data is Map) {
          final errors = data['errors'];
          message =
              (data['message'] ??
                      data['title'] ??
                      (errors is List && errors.isNotEmpty
                          ? errors.first
                          : null))
                  ?.toString() ??
              message;
        } else if (e.type == DioExceptionType.connectionTimeout ||
            e.type == DioExceptionType.receiveTimeout) {
          message = 'Máy chủ phản hồi quá chậm. Vui lòng thử lại.';
        }
        return handler.reject(
          DioException(
            requestOptions: e.requestOptions,
            response: e.response,
            type: e.type,
            error: ApiClientException(
              message,
              statusCode: e.response?.statusCode,
            ),
          ),
        );
      },
    ),
  );

  return dio;
});

final apiClientProvider = Provider<ApiClient>((ref) {
  final dio = ref.watch(dioProvider);
  return ApiClient(dio);
});

class ApiClient {
  final Dio dio;

  ApiClient(this.dio);

  static dynamic unwrap(dynamic responseData) {
    if (responseData is Map<String, dynamic> &&
        responseData.containsKey('data')) {
      if (responseData['success'] == false) {
        throw ApiClientException(
          responseData['message']?.toString() ?? 'Yêu cầu không thành công.',
        );
      }
      return responseData['data'];
    }
    return responseData;
  }

  static ApiClientException readableError(Object error) {
    if (error is ApiClientException) return error;
    if (error is DioException && error.error is ApiClientException) {
      return error.error! as ApiClientException;
    }
    return const ApiClientException(
      'Không thể tải dữ liệu. Vui lòng kiểm tra kết nối và thử lại.',
    );
  }

  // Helper methods for requests
  Future<Response<T>> get<T>(
    String path, {
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    return dio.get<T>(path, queryParameters: queryParameters, options: options);
  }

  Future<Response<T>> post<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    return dio.post<T>(
      path,
      data: data,
      queryParameters: queryParameters,
      options: options,
    );
  }

  Future<Response<T>> put<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    return dio.put<T>(
      path,
      data: data,
      queryParameters: queryParameters,
      options: options,
    );
  }

  Future<Response<T>> patch<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    return dio.patch<T>(
      path,
      data: data,
      queryParameters: queryParameters,
      options: options,
    );
  }

  Future<Response<T>> delete<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    return dio.delete<T>(
      path,
      data: data,
      queryParameters: queryParameters,
      options: options,
    );
  }
}
