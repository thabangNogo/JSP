import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:path_provider/path_provider.dart';

/// Persists signature PNGs under the app documents directory.
class SignatureStorage {
  static Future<Directory> _signaturesDirectory() async {
    final documents = await getApplicationDocumentsDirectory();
    await Directory(documents.path).create(recursive: true);
    final signaturesDir = Directory('${documents.path}/signatures');
    if (!await signaturesDir.exists()) {
      await signaturesDir.create(recursive: true);
    }
    return signaturesDir;
  }

  static Future<File> fileForDraft(String draftId) async {
    final dir = await _signaturesDirectory();
    return File('${dir.path}/signature_$draftId.png');
  }

  static Future<String?> savePng(String draftId, Uint8List bytes) async {
    if (bytes.isEmpty) return null;
    try {
      final file = await fileForDraft(draftId);
      await file.writeAsBytes(bytes, flush: true);
      return file.path;
    } on FileSystemException catch (e, st) {
      debugPrint('SignatureStorage.savePng failed: $e\n$st');
      return null;
    }
  }

  static Future<bool> exists(String path) async {
    try {
      return File(path).exists();
    } catch (_) {
      return false;
    }
  }
}
