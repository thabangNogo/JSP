import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import '../../../../core/utils/dio_error_message.dart';
import '../../../jsa/presentation/providers/jsa_providers.dart';
import '../../../jsa/presentation/providers/work_lookups_provider.dart';
import '../../../jsa/presentation/widgets/glove_friendly/glove_friendly_controls.dart';
import '../providers/safety_providers.dart';
import 'injury_list_screen.dart';

class CaptureInjuryScreen extends ConsumerStatefulWidget {
  const CaptureInjuryScreen({super.key});

  @override
  ConsumerState<CaptureInjuryScreen> createState() => _CaptureInjuryScreenState();
}

class _CaptureInjuryScreenState extends ConsumerState<CaptureInjuryScreen> {
  final _formKey = GlobalKey<FormState>();
  final _employeeController = TextEditingController();
  final _departmentController = TextEditingController();
  final _descriptionController = TextEditingController();
  final _immediateActionController = TextEditingController();
  final _rootCauseController = TextEditingController();
  final _correctiveActionController = TextEditingController();
  final _witnessesController = TextEditingController();
  final _lostTimeController = TextEditingController();

  String? _locationId;
  String? _sectionId;
  String? _injuryType;
  String? _bodyPart;
  String? _status = 'Open';
  DateTime _occurredAt = DateTime.now();
  final List<String> _photoPaths = [];
  final List<String> _attachmentPaths = [];
  bool _submitting = false;

  static const _injuryTypes = [
    ('FirstAidInjury', 'First Aid Injury'),
    ('MedicalTreatmentInjury', 'Medical Treatment Injury'),
    ('LostTimeInjury', 'Lost Time Injury'),
    ('Fatality', 'Fatality'),
  ];

  static const _bodyParts = [
    ('Head', 'Head'),
    ('Eye', 'Eye'),
    ('Face', 'Face'),
    ('Neck', 'Neck'),
    ('Shoulder', 'Shoulder'),
    ('Arm', 'Arm'),
    ('Hand', 'Hand'),
    ('Finger', 'Finger'),
    ('Chest', 'Chest'),
    ('Back', 'Back'),
    ('Leg', 'Leg'),
    ('Knee', 'Knee'),
    ('Foot', 'Foot'),
    ('Multiple', 'Multiple'),
    ('Other', 'Other'),
  ];

  static const _statuses = [
    ('Open', 'Open'),
    ('Closed', 'Closed'),
    ('UnderInvestigation', 'Under Investigation'),
  ];

  @override
  void dispose() {
    _employeeController.dispose();
    _departmentController.dispose();
    _descriptionController.dispose();
    _immediateActionController.dispose();
    _rootCauseController.dispose();
    _correctiveActionController.dispose();
    _witnessesController.dispose();
    _lostTimeController.dispose();
    super.dispose();
  }

  Future<void> _pickDateTime() async {
    final date = await showDatePicker(
      context: context,
      initialDate: _occurredAt,
      firstDate: DateTime(2020),
      lastDate: DateTime.now(),
    );
    if (date == null || !mounted) return;
    final time = await showTimePicker(
      context: context,
      initialTime: TimeOfDay.fromDateTime(_occurredAt),
    );
    if (time == null) return;
    setState(() {
      _occurredAt = DateTime(date.year, date.month, date.day, time.hour, time.minute);
    });
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

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (_injuryType == null || _bodyPart == null || _locationId == null || _sectionId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please complete all required fields.')),
      );
      return;
    }

    setState(() => _submitting = true);
    try {
      final lookups = ref.read(workLookupsProvider).value;
      final location = lookups?.locations.where((l) => l.id == _locationId).firstOrNull?.name ??
          _locationId!;
      final section = lookups?.sections.where((s) => s.id == _sectionId).firstOrNull?.name ??
          _sectionId!;

      final payload = <String, dynamic>{
        'employeeName': _employeeController.text.trim(),
        'department': _departmentController.text.trim(),
        'location': location,
        'section': section,
        'injuryOccurredAt': _occurredAt.toUtc().toIso8601String(),
        'injuryType': _injuryType,
        'bodyPartInjured': _bodyPart,
        'incidentDescription': _descriptionController.text.trim(),
        'immediateActionTaken': _immediateActionController.text.trim().isEmpty
            ? null
            : _immediateActionController.text.trim(),
        'rootCause':
            _rootCauseController.text.trim().isEmpty ? null : _rootCauseController.text.trim(),
        'correctiveAction': _correctiveActionController.text.trim().isEmpty
            ? null
            : _correctiveActionController.text.trim(),
        'witnesses':
            _witnessesController.text.trim().isEmpty ? null : _witnessesController.text.trim(),
        'status': _status,
      };

      if (_lostTimeController.text.trim().isNotEmpty) {
        payload['lostTimeDays'] = int.tryParse(_lostTimeController.text.trim());
      }
      if (_photoPaths.isNotEmpty) payload['photoStoragePaths'] = _photoPaths;
      if (_attachmentPaths.isNotEmpty) payload['attachmentStoragePaths'] = _attachmentPaths;

      await ref.read(safetyRemoteDataSourceProvider).createInjury(payload);
      ref.invalidate(injuriesListProvider);
      await ref.read(injuryFreeDaysProvider.notifier).load();

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Injury submitted. Injury Free Days reset to 0.')),
        );
        context.pop();
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(dioErrorMessage(e))),
        );
      }
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final lookupsAsync = ref.watch(workLookupsProvider);
    const itemStyle = TextStyle(color: Colors.black87, fontSize: 16);

    return Scaffold(
      appBar: AppBar(title: const Text('Capture Injury')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            TextFormField(
              controller: _employeeController,
              decoration: const InputDecoration(labelText: 'Employee *'),
              validator: (v) => v != null && v.trim().isNotEmpty ? null : 'Required',
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _departmentController,
              decoration: const InputDecoration(labelText: 'Department *'),
              validator: (v) => v != null && v.trim().isNotEmpty ? null : 'Required',
            ),
            const SizedBox(height: 12),
            lookupsAsync.when(
              loading: () => const LinearProgressIndicator(),
              error: (e, _) => Text(dioErrorMessage(e)),
              data: (lookups) => Column(
                children: [
                  GloveFriendlyDropdown(
                    label: 'Location *',
                    value: _locationId,
                    items: lookups.locations,
                    onChanged: (v) => setState(() => _locationId = v),
                  ),
                  const SizedBox(height: 12),
                  GloveFriendlyDropdown(
                    label: 'Section *',
                    value: _sectionId,
                    items: lookups.sections,
                    onChanged: (v) => setState(() => _sectionId = v),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 12),
            ListTile(
              contentPadding: EdgeInsets.zero,
              title: const Text('Date & Time *'),
              subtitle: Text(_occurredAt.toLocal().toString().substring(0, 16)),
              trailing: const Icon(Icons.calendar_today),
              onTap: _pickDateTime,
            ),
            const SizedBox(height: 12),
            DropdownButtonFormField<String>(
              value: _injuryType,
              decoration: const InputDecoration(labelText: 'Injury Type *', border: OutlineInputBorder()),
              items: _injuryTypes
                  .map((c) => DropdownMenuItem(value: c.$1, child: Text(c.$2, style: itemStyle)))
                  .toList(),
              onChanged: (v) => setState(() => _injuryType = v),
            ),
            const SizedBox(height: 12),
            DropdownButtonFormField<String>(
              value: _bodyPart,
              decoration: const InputDecoration(labelText: 'Body Part Injured *', border: OutlineInputBorder()),
              items: _bodyParts
                  .map((c) => DropdownMenuItem(value: c.$1, child: Text(c.$2, style: itemStyle)))
                  .toList(),
              onChanged: (v) => setState(() => _bodyPart = v),
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _descriptionController,
              decoration: const InputDecoration(labelText: 'Incident Description *'),
              maxLines: 4,
              validator: (v) => v != null && v.trim().isNotEmpty ? null : 'Required',
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _immediateActionController,
              decoration: const InputDecoration(labelText: 'Immediate Action Taken'),
              maxLines: 2,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _rootCauseController,
              decoration: const InputDecoration(labelText: 'Root Cause'),
              maxLines: 2,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _correctiveActionController,
              decoration: const InputDecoration(labelText: 'Corrective Action'),
              maxLines: 2,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _lostTimeController,
              decoration: const InputDecoration(labelText: 'Lost Time Days'),
              keyboardType: TextInputType.number,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _witnessesController,
              decoration: const InputDecoration(labelText: 'Witnesses'),
              maxLines: 2,
            ),
            const SizedBox(height: 12),
            DropdownButtonFormField<String>(
              value: _status,
              decoration: const InputDecoration(labelText: 'Status', border: OutlineInputBorder()),
              items: _statuses
                  .map((c) => DropdownMenuItem(value: c.$1, child: Text(c.$2, style: itemStyle)))
                  .toList(),
              onChanged: (v) => setState(() => _status = v),
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                const Text('Photos', style: TextStyle(fontWeight: FontWeight.bold)),
                const Spacer(),
                TextButton.icon(
                  onPressed: _addPhoto,
                  icon: const Icon(Icons.add_a_photo),
                  label: const Text('Add'),
                ),
              ],
            ),
            if (_photoPaths.isNotEmpty)
              SizedBox(
                height: 88,
                child: ListView.separated(
                  scrollDirection: Axis.horizontal,
                  itemCount: _photoPaths.length,
                  separatorBuilder: (_, __) => const SizedBox(width: 8),
                  itemBuilder: (context, index) => Stack(
                    children: [
                      ClipRRect(
                        borderRadius: BorderRadius.circular(8),
                        child: Image.file(File(_photoPaths[index]), width: 88, height: 88, fit: BoxFit.cover),
                      ),
                      Positioned(
                        right: 0,
                        child: IconButton(
                          icon: const Icon(Icons.close, color: Colors.white),
                          onPressed: () => setState(() => _photoPaths.removeAt(index)),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            const SizedBox(height: 24),
            ElevatedButton(
              onPressed: _submitting ? null : _submit,
              child: _submitting
                  ? const SizedBox(
                      height: 22,
                      width: 22,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Text('Submit Injury'),
            ),
          ],
        ),
      ),
    );
  }
}
