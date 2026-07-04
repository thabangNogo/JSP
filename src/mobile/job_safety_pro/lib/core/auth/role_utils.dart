import '../../features/auth/data/models/auth_models.dart';

const kSafetyManagerRole = 'Safety Manager';
const kSafetyOfficerRole = 'Safety Officer';

bool isSafetyLead(UserModel? user) =>
    user?.roles.any((r) => r == kSafetyManagerRole || r == kSafetyOfficerRole) ?? false;
