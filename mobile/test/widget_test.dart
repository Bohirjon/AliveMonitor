import 'package:flutter_test/flutter_test.dart';

import 'package:alive_monitor/main.dart';

void main() {
  testWidgets('App builds without error', (WidgetTester tester) async {
    await tester.pumpWidget(const AliveMonitorApp());
    await tester.pump();
  });
}
