import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import '../../../jsa/presentation/providers/work_lookups_provider.dart';
import '../../../jsa/presentation/widgets/glove_friendly/glove_friendly_controls.dart';
import '../providers/safety_providers.dart';

class StopUnsafeWorkScreen extends ConsumerStatefulWidget {
  const StopUnsafeWorkScreen({super.key});

  @override
  ConsumerState<StopUnsafeWorkScreen> createState() => _StopUnsafeWorkScreenState();
}

class _StopUnsafeWorkScreenState extends ConsumerState<StopUnsafeWorkScreen> {
  final _formKey = GlobalKey<FormState>();
  final _descriptionController = TextEditingController();
  final _actionsController = TextEditingController();
  String? _locationId;
  String? _sectionId;
  String? _category;
  String? _immediateRisk;
  final List<String> _photoPaths = [];
  bool _submitting = false;

  static const _categories = [
    ('UnsafeCondition', 'Unsafe Condition'),
    ('UnsafeAct', 'Unsafe Act'),
    ('EquipmentFailure', 'Equipment Failure'),
    ('ElectricalHazard', 'Electrical Hazard'),
    ('WorkingAtHeight', 'Working At Height'),
    ('FallingObject', 'Falling Object'),
    ('FireHazard', 'Fire Hazard'),
    ('ChemicalHazard', 'Chemical Hazard'),
    ('EnvironmentalHazard', 'Environmental Hazard'),
    ('Other', 'Other'),
  ];

  @override
  void dispose() {
    _descriptionController.dispose();
    _actionsController.dispose();
    super.dispose();
  }

  Future<void> _addPhoto() async {
    final picker = ImagePicker();
    final image = await picker.pickImage(source: ImageSource.camera);
    if (image != null) setState(() => _photoPaths.add(image.path));
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (_locationId == null || _sectionId == null || _category == null || _immediateRisk == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Complete all required fields.')),
      );
      return;
    }

    final lookups = await ref.read(workLookupsProvider.future);
    final location = lookups.locations.firstWhere((l) => l.id == _locationId).name;
    final section = lookups.sections.firstWhere((s) => s.id == _sectionId).name;

    setState(() => _submitting = true);
    try {
      await ref.read(safetyRemoteDataSourceProvider).submitStopUnsafeWork({
        'location': location,
        'section': section,
        'category': _category,
        'description': _descriptionController.text.trim(),
        'immediateRisk': _immediateRisk,
        'actionsTaken': _actionsController.text.trim(),
        'photoStoragePaths': _photoPaths,
      });
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Stop unsafe work report submitted. Safety team notified.')),
      );
      context.pop();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Could not submit: $e')),
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
      appBar: AppBar(title: const Text('Stop Unsafe Work')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            const Text(
              'Report dangerous conditions that require work to stop immediately.',
              style: TextStyle(fontSize: 16),
            ),
            const SizedBox(height: 16),
            lookupsAsync.when(
              loading: () => const LinearProgressIndicator(),
              error: (e, _) => Text('Could not load locations: $e'),
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
              decoration: const InputDecoration(labelText: 'Description *', border: OutlineInputBorder()),
              minLines: 3,
              maxLines: 6,
              validator: (v) => v == null || v.trim().isEmpty ? 'Required' : null,
            ),
            const SizedBox(height: 16),
            DropdownButtonFormField<String>(
              value: _immediateRisk,
              decoration: const InputDecoration(labelText: 'Immediate Risk *', border: OutlineInputBorder()),
              dropdownColor: Colors.white,
              style: itemStyle,
              items: const [
                DropdownMenuItem(value: 'High', child: Text('High')),
                DropdownMenuItem(value: 'Critical', child: Text('Critical')),
              ],
              onChanged: (v) => setState(() => _immediateRisk = v),
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _actionsController,
              decoration: const InputDecoration(labelText: 'Actions Taken', border: OutlineInputBorder()),
              minLines: 2,
              maxLines: 4,
            ),
            const SizedBox(height: 12),
            Wrap(
              spacing: 8,
              children: [
                OutlinedButton.icon(
                  onPressed: _addPhoto,
                  icon: const Icon(Icons.camera_alt),
                  label: const Text('Add Photo'),
                ),
                Text('${_photoPaths.length} photo(s)'),
              ],
            ),
            const SizedBox(height: 24),
            SizedBox(
              width: double.infinity,
              height: 52,
              child: ElevatedButton(
                onPressed: _submitting ? null : _submit,
                child: _submitting
                    ? const CircularProgressIndicator()
                    : const Text('Submit Report', style: TextStyle(fontSize: 18)),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
