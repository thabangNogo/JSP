import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../jsa/presentation/providers/work_lookups_provider.dart';
import '../../data/models/employee_profile_model.dart';
import '../providers/employee_profile_provider.dart';

class EditProfileScreen extends ConsumerStatefulWidget {
  const EditProfileScreen({super.key});

  @override
  ConsumerState<EditProfileScreen> createState() => _EditProfileScreenState();
}

class _EditProfileScreenState extends ConsumerState<EditProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _nameController;
  late final TextEditingController _surnameController;
  late final TextEditingController _companyNumberController;
  late final TextEditingController _occupationController;
  String? _workDepartmentId;

  @override
  void initState() {
    super.initState();
    final profile = ref.read(employeeProfileProvider).profile;
    _workDepartmentId = profile?.workDepartmentId;
    _nameController = TextEditingController(text: profile?.name ?? '');
    _surnameController = TextEditingController(text: profile?.surname ?? '');
    _companyNumberController = TextEditingController(text: profile?.companyNumber ?? '');
    _occupationController = TextEditingController(text: profile?.occupation ?? '');
  }

  @override
  void dispose() {
    _nameController.dispose();
    _surnameController.dispose();
    _companyNumberController.dispose();
    _occupationController.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;

    if (_workDepartmentId == null || _workDepartmentId!.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Department is required')),
      );
      return;
    }

    final lookups = await ref.read(workLookupsProvider.future);
    final departmentName = lookups.departments
        .firstWhere((d) => d.id == _workDepartmentId)
        .name;

    final existing = ref.read(employeeProfileProvider).profile;
    final profile = EmployeeProfileModel(
      id: existing?.id,
      userId: existing?.userId,
      workDepartmentId: _workDepartmentId,
      workDepartmentName: departmentName,
      name: _nameController.text.trim(),
      surname: _surnameController.text.trim(),
      companyNumber: _companyNumberController.text.trim(),
      occupation: _occupationController.text.trim(),
    );

    final ok = await ref.read(employeeProfileProvider.notifier).saveProfile(profile);
    if (!mounted) return;

    if (ok) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Profile saved')),
      );
      context.pop();
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            ref.read(employeeProfileProvider).error ?? 'Could not save profile',
          ),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final isLoading = ref.watch(employeeProfileProvider).isLoading;
    final lookupsAsync = ref.watch(workLookupsProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('Save Profile')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            TextFormField(
              controller: _nameController,
              decoration: const InputDecoration(labelText: 'Name *'),
              validator: (v) =>
                  v == null || v.trim().isEmpty ? 'Name is required' : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _surnameController,
              decoration: const InputDecoration(labelText: 'Surname *'),
              validator: (v) =>
                  v == null || v.trim().isEmpty ? 'Surname is required' : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _companyNumberController,
              decoration: const InputDecoration(labelText: 'Company Number *'),
              validator: (v) => v == null || v.trim().isEmpty
                  ? 'Company number is required'
                  : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _occupationController,
              decoration: const InputDecoration(labelText: 'Occupation *'),
              validator: (v) =>
                  v == null || v.trim().isEmpty ? 'Occupation is required' : null,
            ),
            const SizedBox(height: 12),
            lookupsAsync.when(
              loading: () => const LinearProgressIndicator(),
              error: (e, _) => Text('Could not load departments: $e'),
              data: (lookups) {
                final textColor = Theme.of(context).colorScheme.onSurface;
                final itemStyle = TextStyle(fontSize: 16, color: textColor);

                return DropdownButtonFormField<String>(
                  value: _workDepartmentId,
                  decoration: const InputDecoration(labelText: 'Department *'),
                  dropdownColor: Colors.white,
                  style: itemStyle,
                  isExpanded: true,
                  items: lookups.departments
                      .map(
                        (d) => DropdownMenuItem<String>(
                          value: d.id,
                          child: Text(d.name, style: itemStyle),
                        ),
                      )
                      .toList(),
                  onChanged: (value) => setState(() => _workDepartmentId = value),
                );
              },
            ),
            const SizedBox(height: 24),
            SizedBox(
              width: double.infinity,
              height: 48,
              child: ElevatedButton(
                onPressed: isLoading ? null : _save,
                child: isLoading
                    ? const SizedBox(
                        height: 24,
                        width: 24,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text('Save Profile'),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
