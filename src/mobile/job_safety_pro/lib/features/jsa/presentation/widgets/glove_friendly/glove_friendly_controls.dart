import 'package:flutter/material.dart';
import '../../../data/models/work_lookup_models.dart';
import '../../../domain/models/quick_assessment_models.dart';

/// Minimum touch target for operators wearing gloves (Material 48dp+).
const double kGloveMinTouchHeight = 56;
const double kGloveHorizontalPadding = 20;
const double kGloveSectionSpacing = 24;

class GloveFriendlyYesNoSelector extends StatelessWidget {
  const GloveFriendlyYesNoSelector({
    super.key,
    required this.selected,
    required this.onChanged,
    this.errorText,
  });

  final YesNoAnswer? selected;
  final ValueChanged<YesNoAnswer> onChanged;
  final String? errorText;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Row(
          children: [
            Expanded(
              child: _AnswerButton(
                label: 'Yes',
                icon: Icons.check_circle_outline,
                isSelected: selected == YesNoAnswer.yes,
                color: Colors.green.shade700,
                onTap: () => onChanged(YesNoAnswer.yes),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: _AnswerButton(
                label: 'No',
                icon: Icons.cancel_outlined,
                isSelected: selected == YesNoAnswer.no,
                color: Colors.orange.shade800,
                onTap: () => onChanged(YesNoAnswer.no),
              ),
            ),
          ],
        ),
        if (errorText != null) ...[
          const SizedBox(height: 8),
          Text(
            errorText!,
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.error,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ],
    );
  }
}

class _AnswerButton extends StatelessWidget {
  const _AnswerButton({
    required this.label,
    required this.icon,
    required this.isSelected,
    required this.color,
    required this.onTap,
  });

  final String label;
  final IconData icon;
  final bool isSelected;
  final Color color;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: isSelected ? color.withValues(alpha: 0.15) : Colors.grey.shade100,
      borderRadius: BorderRadius.circular(16),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(16),
        child: Container(
          height: kGloveMinTouchHeight,
          alignment: Alignment.center,
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
              color: isSelected ? color : Colors.grey.shade400,
              width: isSelected ? 3 : 1.5,
            ),
          ),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(icon, color: isSelected ? color : Colors.grey.shade700, size: 28),
              const SizedBox(width: 8),
              Text(
                label,
                style: TextStyle(
                  fontSize: 20,
                  fontWeight: FontWeight.w700,
                  color: isSelected ? color : Colors.grey.shade800,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class GloveFriendlyActionTakenSelector extends StatelessWidget {
  const GloveFriendlyActionTakenSelector({
    super.key,
    required this.selected,
    required this.onSelected,
    required this.otherComment,
    required this.onOtherCommentChanged,
    this.actionError,
    this.otherError,
  });

  final NoActionTaken? selected;
  final ValueChanged<NoActionTaken> onSelected;
  final String otherComment;
  final ValueChanged<String> onOtherCommentChanged;
  final String? actionError;
  final String? otherError;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text(
          'Action taken',
          style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700),
        ),
        const SizedBox(height: 12),
        ...NoActionTaken.values.map(
          (action) => Padding(
            padding: const EdgeInsets.only(bottom: 10),
            child: _ActionChip(
              label: action.label,
              isSelected: selected == action,
              onTap: () => onSelected(action),
            ),
          ),
        ),
        if (actionError != null) ...[
          Text(
            actionError!,
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.error,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 8),
        ],
        if (selected == NoActionTaken.other) ...[
          const SizedBox(height: 8),
          _OtherCommentField(
            key: ValueKey(otherComment),
            initialValue: otherComment,
            errorText: otherError,
            onChanged: onOtherCommentChanged,
          ),
        ],
      ],
    );
  }
}

class _ActionChip extends StatelessWidget {
  const _ActionChip({
    required this.label,
    required this.isSelected,
    required this.onTap,
  });

  final String label;
  final bool isSelected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final primary = Theme.of(context).colorScheme.primary;

    return Material(
      color: isSelected ? primary.withValues(alpha: 0.12) : Colors.white,
      borderRadius: BorderRadius.circular(14),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(14),
        child: Container(
          width: double.infinity,
          constraints: const BoxConstraints(minHeight: kGloveMinTouchHeight),
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(14),
            border: Border.all(
              color: isSelected ? primary : Colors.grey.shade400,
              width: isSelected ? 2.5 : 1.5,
            ),
          ),
          alignment: Alignment.centerLeft,
          child: Text(
            label,
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w600,
              color: isSelected ? primary : Colors.grey.shade900,
            ),
          ),
        ),
      ),
    );
  }
}

class _OtherCommentField extends StatefulWidget {
  const _OtherCommentField({
    super.key,
    required this.initialValue,
    required this.onChanged,
    this.errorText,
  });

  final String initialValue;
  final ValueChanged<String> onChanged;
  final String? errorText;

  @override
  State<_OtherCommentField> createState() => _OtherCommentFieldState();
}

class _OtherCommentFieldState extends State<_OtherCommentField> {
  late final TextEditingController _controller;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: widget.initialValue);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return TextField(
      controller: _controller,
      onChanged: widget.onChanged,
      minLines: 3,
      maxLines: 5,
      style: const TextStyle(fontSize: 18),
      decoration: InputDecoration(
        labelText: 'Describe action taken',
        hintText: 'Enter details...',
        errorText: widget.errorText,
        contentPadding: const EdgeInsets.all(20),
      ),
    );
  }
}

class GloveFriendlyDropdown extends StatelessWidget {
  const GloveFriendlyDropdown({
    super.key,
    required this.label,
    required this.value,
    required this.items,
    required this.onChanged,
    this.errorText,
  });

  final String label;
  final String? value;
  final List<WorkLookupItem> items;
  final ValueChanged<String?> onChanged;
  final String? errorText;

  @override
  Widget build(BuildContext context) {
    final textColor = Theme.of(context).colorScheme.onSurface;
    final itemStyle = TextStyle(fontSize: 18, color: textColor, fontWeight: FontWeight.w500);

    return DropdownButtonFormField<String>(
      value: value != null && value!.isNotEmpty ? value : null,
      decoration: InputDecoration(
        labelText: '$label *',
        errorText: errorText,
        contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 20),
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
      ),
      dropdownColor: Colors.white,
      style: itemStyle,
      isExpanded: true,
      items: items
          .map(
            (item) => DropdownMenuItem<String>(
              value: item.id,
              child: Text(item.name, style: itemStyle),
            ),
          )
          .toList(),
      onChanged: onChanged,
    );
  }
}

class GloveFriendlyRequiredTextField extends StatelessWidget {
  const GloveFriendlyRequiredTextField({
    super.key,
    required this.label,
    required this.controller,
    required this.onChanged,
    this.errorText,
    this.textInputAction = TextInputAction.next,
  });

  final String label;
  final TextEditingController controller;
  final ValueChanged<String> onChanged;
  final String? errorText;
  final TextInputAction textInputAction;

  @override
  Widget build(BuildContext context) {
    return TextField(
      controller: controller,
      onChanged: onChanged,
      textInputAction: textInputAction,
      style: const TextStyle(fontSize: 18),
      decoration: InputDecoration(
        labelText: '$label *',
        errorText: errorText,
        contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 20),
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
      ),
    );
  }
}

class QuickAssessmentQuestionCard extends StatelessWidget {
  const QuickAssessmentQuestionCard({
    super.key,
    required this.questionNumber,
    required this.questionText,
    required this.response,
    required this.onAnswerChanged,
    required this.onActionChanged,
    required this.onOtherCommentChanged,
    this.answerError,
    this.actionError,
    this.otherError,
  });

  final int questionNumber;
  final String questionText;
  final QuickAssessmentQuestionResponse response;
  final ValueChanged<YesNoAnswer> onAnswerChanged;
  final ValueChanged<NoActionTaken> onActionChanged;
  final ValueChanged<String> onOtherCommentChanged;
  final String? answerError;
  final String? actionError;
  final String? otherError;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Card(
      margin: EdgeInsets.zero,
      child: Padding(
        padding: const EdgeInsets.all(kGloveHorizontalPadding),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                CircleAvatar(
                  radius: 18,
                  backgroundColor: theme.colorScheme.primary,
                  child: Text(
                    '$questionNumber',
                    style: const TextStyle(
                      color: Colors.white,
                      fontWeight: FontWeight.bold,
                      fontSize: 16,
                    ),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    questionText,
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontSize: 18,
                      fontWeight: FontWeight.w600,
                      height: 1.35,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 20),
            GloveFriendlyYesNoSelector(
              selected: response.answer,
              onChanged: onAnswerChanged,
              errorText: answerError,
            ),
            if (response.requiresAction) ...[
              const SizedBox(height: kGloveSectionSpacing),
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.orange.shade50,
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: Colors.orange.shade200),
                ),
                child: GloveFriendlyActionTakenSelector(
                  selected: response.actionTaken,
                  onSelected: onActionChanged,
                  otherComment: response.otherComment,
                  onOtherCommentChanged: onOtherCommentChanged,
                  actionError: actionError,
                  otherError: otherError,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
