class User {
  final String id;
  final String email;
  final String name;
  final String? avatarUrl;
  final String alertEmail;
  final bool telegramLinked;
  final String? webhookUrl;

  User({
    required this.id,
    required this.email,
    required this.name,
    this.avatarUrl,
    required this.alertEmail,
    required this.telegramLinked,
    this.webhookUrl,
  });

  factory User.fromJson(Map<String, dynamic> json) => User(
        id: json['id'] as String,
        email: json['email'] as String,
        name: json['name'] as String,
        avatarUrl: json['avatarUrl'] as String?,
        alertEmail: json['alertEmail'] as String? ?? '',
        telegramLinked: json['telegramLinked'] as bool? ?? false,
        webhookUrl: json['webhookUrl'] as String?,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'email': email,
        'name': name,
        'avatarUrl': avatarUrl,
        'alertEmail': alertEmail,
        'telegramLinked': telegramLinked,
        'webhookUrl': webhookUrl,
      };

  User copyWith({
    String? alertEmail,
    bool? telegramLinked,
    String? webhookUrl,
  }) =>
      User(
        id: id,
        email: email,
        name: name,
        avatarUrl: avatarUrl,
        alertEmail: alertEmail ?? this.alertEmail,
        telegramLinked: telegramLinked ?? this.telegramLinked,
        webhookUrl: webhookUrl ?? this.webhookUrl,
      );
}
