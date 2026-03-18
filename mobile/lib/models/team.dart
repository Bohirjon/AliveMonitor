class Team {
  final String id;
  final String name;
  final List<String> memberEmails;
  final bool telegramLinked;
  final String? webhookUrl;
  final DateTime createdAt;
  final DateTime updatedAt;

  Team({
    required this.id,
    required this.name,
    required this.memberEmails,
    required this.telegramLinked,
    this.webhookUrl,
    required this.createdAt,
    required this.updatedAt,
  });

  factory Team.fromJson(Map<String, dynamic> json) => Team(
        id: json['id'] as String,
        name: json['name'] as String,
        memberEmails: List<String>.from(json['memberEmails'] as List),
        telegramLinked: json['telegramLinked'] as bool? ?? false,
        webhookUrl: json['webhookUrl'] as String?,
        createdAt: DateTime.parse(json['createdAt'] as String),
        updatedAt: DateTime.parse(json['updatedAt'] as String),
      );
}

class CreateTeamRequest {
  final String name;
  final List<String> memberEmails;
  final String? webhookUrl;

  CreateTeamRequest({required this.name, required this.memberEmails, this.webhookUrl});

  Map<String, dynamic> toJson() => {
        'name': name,
        'memberEmails': memberEmails,
        if (webhookUrl != null) 'webhookUrl': webhookUrl,
      };
}

class LinkCodeResponse {
  final String code;
  final String deepLink;
  final DateTime expiresAt;

  LinkCodeResponse({
    required this.code,
    required this.deepLink,
    required this.expiresAt,
  });

  factory LinkCodeResponse.fromJson(Map<String, dynamic> json) =>
      LinkCodeResponse(
        code: json['code'] as String,
        deepLink: json['deepLink'] as String,
        expiresAt: DateTime.parse(json['expiresAt'] as String),
      );
}

class TelegramStatusResponse {
  final bool isLinked;
  final String? chatId;

  TelegramStatusResponse({required this.isLinked, this.chatId});

  factory TelegramStatusResponse.fromJson(Map<String, dynamic> json) =>
      TelegramStatusResponse(
        isLinked: json['isLinked'] as bool,
        chatId: json['chatId'] as String?,
      );
}
