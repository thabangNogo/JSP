import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import '../../../../core/utils/dio_error_message.dart';
import '../../../auth/presentation/providers/auth_provider.dart';
import '../../../jsa/presentation/providers/jsa_providers.dart';
import '../../../jsa/presentation/providers/work_lookups_provider.dart';
import '../../../jsa/presentation/widgets/glove_friendly/glove_friendly_controls.dart';
import '../providers/safety_providers.dart';

class ReportNearMissScreen extends ConsumerStatefulWidget {
  const ReportNearMissScreen({super.key});

  @override
  ConsumerState<ReportNearMissScreen> createState() => _ReportNearMissScreenState();
}

class _ReportNearMissScreenState extends ConsumerState<ReportNearMissScreen> {
  final _formKey = GlobalKey<FormState>();
  final _descriptionController = TextEditingController();
  String? _locationId;
  String? _sectionId;
  String? _category;
  final List<String> _photoPaths = [];
  bool _submitting = false;

  static const _categories = [
    ('SlipTripFall', 'Slip / Trip / Fall'),
    ('StruckBy', 'Struck By'),
    ('CaughtIn', 'Caught In / Between'),
    ('Electrical', 'Electrical'),
    ('Fire', 'Fire'),
    ('Chemical', 'Chemical'),
    ('Ergonomic', 'Ergonomic'),
    ('Vehicle', 'Vehicle'),
    ('Equipment', 'Equipment'),
    ('Other', 'Other'),
  ];

  @override
  void dispose() {
    _descriptionController.dispose();
    super.dispose();
  }

  Future<void> _addPhoto() async {
    final source = await showModalBottomSheet<ImageSource>(
      context: context,
      builder: (ctx) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.camera_alt),
              title: const Text('Take photo'),
              onTap: () => Navigator.pop(ctx, ImageSource.camera),
            ),
            ListTile(
              leading: const Icon(Icons.photo_library),
              title: const Text('Choose from gallery'),
              onTap: () => Navigator.pop(ctx, ImageSource.gallery),
            ),
          ],
        ),
      ),
    );
    if (source == null || !mounted) return;

    final photoService = ref.read(photoCaptureServiceProvider);
    final path = source == ImageSource.camera
        ? await photoService.captureFromCamera()
        : await photoService.pickFromGallery();
    if (path != null) setState(() => _photoPaths.add(path));
  }

  void _removePhoto(int index) {
    setState(() => _photoPaths.removeAt(index));
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    final user = ref.read(authProvider).user;
    if (user == null ||
        _locationId == null ||
        _sectionId == null ||
        _category == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Complete all required fields.')),
      );
      return;
    }

    final lookups = await ref.read(workLookupsProvider.future);
    final location = lookups.locations.firstWhere((l) => l.id == _locationId).name;
    final section = lookups.sections.firstWhere((s) => s.id == _sectionId).name;

    final payload = <String, dynamic>{
      'companyId': user.companyId,
      'plantId': user.plantId ?? user.companyId,
      'departmentId': user.departmentId ?? user.companyId,
      'location': location,
      'section': section,
      'category': _category,
      'description': _descriptionController.text.trim(),
      'occurredAt': DateTime.now().toUtc().toIso8601String(),
    };
    if (_photoPaths.isNotEmpty) {
      payload['photoStoragePaths'] = _photoPaths;
    }

    setState(() => _submitting = true);
    try {
      await ref.read(safetyRemoteDataSourceProvider).submitNearMissReport(payload);
      if (!mounted) return;
      ref.invalidate(employeeKpiProvider);
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Near miss reported. Safety officer notified.')),
      );
      context.pop();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Could not submit: ${dioErrorMessage(e)}')),
        );
      }
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final lookupsAsync = ref.watch(workLookupsProvider);
    final textColor = Theme.of(context).colorScheme.onSurface;
    final itemStyle = TextStyle(fontSize: 16, color: textColor);

    return Scaffold(
      appBar: AppBar(title: const Text('Report Near Miss')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            lookupsAsync.when(
              loading: () => const LinearProgressIndicator(),
              error: (e, _) => Text('Error: $e'),
              data: (lookups) => Column(
                children: [
                  GloveFriendlyDropdown(
                    label: 'Location',
                    value: _locationId,
                    items: lookups.locations,
                    onChanged: (v) => setState(() => _locationId = v),
                  ),
                  const SizedBox(height: 16),
                  GloveFriendlyDropdown(
                    label: 'Section',
                    value: _sectionId,
                    items: lookups.sections,
                    onChanged: (v) => setState(() => _sectionId = v),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),
            DropdownButtonFormField<String>(
              value: _category,
              decoration: const InputDecoration(labelText: 'Category *', border: OutlineInputBorder()),
              dropdownColor: Colors.white,
              style: itemStyle,
              items: _categories
                  .map((c) => DropdownMenuItem(value: c.$1, child: Text(c.$2, style: itemStyle)))
                  .toList(),
              onChanged: (v) => setState(() => _category = v),
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _descriptionController,
              decoration: const InputDecoration(labelText: 'What happened? *', border: OutlineInputBorder()),
              minLines: 4,
              maxLines: 8,
              validator: (v) => v == null || v.trim().isEmpty ? 'Required' : null,
            ),
            const SizedBox(height: 16),
            Text(
              'Photos (optional)',
              style: Theme.of(context).textTheme.titleSmall?.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
            ),
            const SizedBox(height: 4),
            Text(
              'Add photos if you have them — you can submit without any.',
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                    color: Colors.grey.shade700,
                  ),
            ),
            const SizedBox(height: 8),
            if (_photoPaths.isNotEmpty)
              SizedBox(
                height: 100,
                child: ListView.separated(
                  scrollDirection: Axis.horizontal,
                  itemCount: _photoPaths.length,
                  separatorBuilder: (_, __) => const SizedBox(width: 8),
                  itemBuilder: (context, index) => Stack(
                    children: [
                      ClipRRect(
                        borderRadius: BorderRadius.circular(8),
                        child: Image.file(
                          File(_photoPaths[index]),
                          width: 100,
                          height: 100,
                          fit: BoxFit.cover,
                        ),
                      ),
                      Positioned(
                        top: 4,
                        right: 4,
                        child: IconButton(
                          style: IconButton.styleFrom(
                            backgroundColor: Colors.black54,
                            foregroundColor: Colors.white,
                            padding: const EdgeInsets.all(4),
                            minimumSize: const Size(32, 32),
                          ),
                          icon: const Icon(Icons.close, size: 18),
                          onPressed: () => _removePhoto(index),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            const SizedBox(height: 8),
            OutlinedButton.icon(
              onPressed: _addPhoto,
              icon: const Icon(Icons.add_a_photo_outlined),
              label: Text(
                _photoPaths.isEmpty
                    ? 'Add photo (optional)'
                    : 'Add another photo (${_photoPaths.length})',
              ),
            ),
            const SizedBox(height: 24),
            SizedBox(
              width: double.infinity,
              height: 52,
              child: ElevatedButton(
                onPressed: _submitting ? null : _submit,
                child: _submitting
                    ? const CircularProgressIndicator()
                    : const Text('Submit Near Miss', style: TextStyle(fontSize: 18)),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
