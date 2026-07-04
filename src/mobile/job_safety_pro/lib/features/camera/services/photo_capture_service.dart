import 'dart:io';

import 'package:camera/camera.dart';
import 'package:image_picker/image_picker.dart';
import 'package:path_provider/path_provider.dart';
import 'package:uuid/uuid.dart';

class PhotoCaptureService {
  PhotoCaptureService({ImagePicker? picker, CameraController? cameraController})
      : _picker = picker ?? ImagePicker();

  final ImagePicker _picker;
  final _uuid = const Uuid();

  Future<String?> captureFromCamera() async {
    final file = await _picker.pickImage(source: ImageSource.camera, imageQuality: 80);
    if (file == null) return null;
    return _persist(file.path);
  }

  Future<String?> pickFromGallery() async {
    final file = await _picker.pickImage(source: ImageSource.gallery, imageQuality: 80);
    if (file == null) return null;
    return _persist(file.path);
  }

  Future<String> _persist(String sourcePath) async {
    final dir = await getApplicationDocumentsDirectory();
    final photosDir = Directory('${dir.path}/jsp_photos');
    if (!await photosDir.exists()) await photosDir.create(recursive: true);
    final target = '${photosDir.path}/${_uuid.v4()}.jpg';
    await File(sourcePath).copy(target);
    return target;
  }
}
