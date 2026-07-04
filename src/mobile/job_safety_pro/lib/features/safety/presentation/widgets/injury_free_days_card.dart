import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../providers/safety_providers.dart';

class InjuryFreeDaysCard extends ConsumerStatefulWidget {
  const InjuryFreeDaysCard({super.key});

  @override
  ConsumerState<InjuryFreeDaysCard> createState() => _InjuryFreeDaysCardState();
}

class _InjuryFreeDaysCardState extends ConsumerState<InjuryFreeDaysCard>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  int _displayedDays = 0;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(vsync: this, duration: const Duration(milliseconds: 900));
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _animateTo(int target) {
    final animation = Tween<double>(begin: _displayedDays.toDouble(), end: target.toDouble())
        .animate(CurvedAnimation(parent: _controller, curve: Curves.easeOutCubic));
    _controller
      ..reset()
      ..forward();
    animation.addListener(() {
      setState(() => _displayedDays = animation.value.round());
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(injuryFreeDaysProvider);

    ref.listen(injuryFreeDaysProvider, (previous, next) {
      if (next.hasValue && next.value != _displayedDays) {
        _animateTo(next.value!);
      }
    });

    if (state.isLoading && _displayedDays == 0) {
      return const Card(
        color: Color(0xFF2E7D32),
        margin: EdgeInsets.only(bottom: 16),
        child: Padding(
          padding: EdgeInsets.all(24),
          child: Center(child: CircularProgressIndicator(color: Colors.white)),
        ),
      );
    }

    final days = state.value ?? 0;
    if (_displayedDays == 0 && days > 0) {
      WidgetsBinding.instance.addPostFrameCallback((_) => _animateTo(days));
    }

    return Card(
      color: const Color(0xFF2E7D32),
      elevation: 4,
      margin: const EdgeInsets.only(bottom: 16),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 28, horizontal: 24),
        child: Column(
          children: [
            const Icon(Icons.health_and_safety, color: Colors.white, size: 48),
            const SizedBox(height: 12),
            Text(
              'INJURY FREE DAYS',
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    color: Colors.white,
                    letterSpacing: 1.2,
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 8),
            Text(
              '$_displayedDays',
              style: Theme.of(context).textTheme.displayLarge?.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.bold,
                    height: 1,
                  ),
            ),
            const SizedBox(height: 8),
            Text(
              days == 1 ? '1 Injury Free Day' : '$days Injury Free Days',
              style: const TextStyle(color: Colors.white70, fontSize: 16),
            ),
            const SizedBox(height: 12),
            const Text(
              'Keep it Safe!',
              style: TextStyle(
                color: Colors.white,
                fontSize: 18,
                fontWeight: FontWeight.w600,
                fontStyle: FontStyle.italic,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
