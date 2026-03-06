import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../models/monitored_endpoint.dart';
import '../../providers/endpoint_provider.dart';
import 'endpoint_card.dart';
import 'endpoint_form_sheet.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  final _searchController = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<EndpointProvider>().fetchEndpoints();
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  void _showAddEndpoint() {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      useSafeArea: true,
      builder: (_) => const EndpointFormSheet(),
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Dashboard'),
        actions: [
          IconButton(
            icon: const Icon(Icons.add),
            onPressed: _showAddEndpoint,
          ),
        ],
      ),
      body: Column(
        children: [
          // Search and filter bar
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search by name or URL...',
                prefixIcon: const Icon(Icons.search),
                suffixIcon: _searchController.text.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.clear),
                        onPressed: () {
                          _searchController.clear();
                          context
                              .read<EndpointProvider>()
                              .setSearchQuery('');
                        },
                      )
                    : null,
                isDense: true,
              ),
              onChanged: (value) {
                context.read<EndpointProvider>().setSearchQuery(value);
                setState(() {});
              },
            ),
          ),
          // Status filter chips
          Consumer<EndpointProvider>(
            builder: (context, provider, _) {
              return SingleChildScrollView(
                scrollDirection: Axis.horizontal,
                padding: const EdgeInsets.symmetric(horizontal: 16),
                child: Row(
                  children: ['All', 'Healthy', 'Unhealthy', 'Disabled']
                      .map((status) {
                    final isSelected = provider.statusFilter == status;
                    return Padding(
                      padding: const EdgeInsets.only(right: 8),
                      child: FilterChip(
                        label: Text(status),
                        selected: isSelected,
                        onSelected: (_) {
                          provider.setStatusFilter(status);
                        },
                      ),
                    );
                  }).toList(),
                ),
              );
            },
          ),
          const SizedBox(height: 8),
          // Endpoints list
          Expanded(
            child: Consumer<EndpointProvider>(
              builder: (context, provider, _) {
                if (provider.isLoading && provider.endpoints.isEmpty) {
                  return const Center(child: CircularProgressIndicator());
                }

                if (provider.error != null && provider.endpoints.isEmpty) {
                  return Center(
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Text(provider.error!,
                            style: TextStyle(
                                color: theme.colorScheme.error)),
                        const SizedBox(height: 16),
                        FilledButton(
                          onPressed: provider.fetchEndpoints,
                          child: const Text('Retry'),
                        ),
                      ],
                    ),
                  );
                }

                if (provider.endpoints.isEmpty) {
                  return Center(
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.monitor_heart_outlined,
                            size: 64,
                            color: theme.colorScheme.onSurfaceVariant),
                        const SizedBox(height: 16),
                        Text(
                          'No endpoints yet',
                          style: theme.textTheme.titleMedium,
                        ),
                        const SizedBox(height: 8),
                        Text(
                          'Add your first endpoint to start monitoring',
                          style: theme.textTheme.bodyMedium?.copyWith(
                            color: theme.colorScheme.onSurfaceVariant,
                          ),
                        ),
                        const SizedBox(height: 24),
                        FilledButton.icon(
                          onPressed: _showAddEndpoint,
                          icon: const Icon(Icons.add),
                          label: const Text('Add Endpoint'),
                        ),
                      ],
                    ),
                  );
                }

                return RefreshIndicator(
                  onRefresh: provider.fetchEndpoints,
                  child: ListView.builder(
                    padding: const EdgeInsets.symmetric(horizontal: 16),
                    itemCount: provider.endpoints.length,
                    itemBuilder: (context, index) {
                      final endpoint = provider.endpoints[index];
                      return EndpointCard(
                        endpoint: endpoint,
                        onTap: () =>
                            context.push('/endpoints/${endpoint.id}'),
                        onToggle: () =>
                            provider.toggleEndpoint(endpoint.id),
                        onEdit: () {
                          showModalBottomSheet(
                            context: context,
                            isScrollControlled: true,
                            useSafeArea: true,
                            builder: (_) => EndpointFormSheet(
                              endpoint: endpoint,
                            ),
                          );
                        },
                        onDelete: () => _confirmDelete(endpoint),
                      );
                    },
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _confirmDelete(MonitoredEndpoint endpoint) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Delete Endpoint'),
        content: Text(
            'Are you sure you want to delete "${endpoint.friendlyName}"?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancel'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            style: FilledButton.styleFrom(
              backgroundColor: Theme.of(context).colorScheme.error,
            ),
            child: const Text('Delete'),
          ),
        ],
      ),
    );

    if (confirmed == true && mounted) {
      await context.read<EndpointProvider>().deleteEndpoint(endpoint.id);
    }
  }
}
