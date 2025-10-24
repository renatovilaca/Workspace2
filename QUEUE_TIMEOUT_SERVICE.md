# Queue Timeout Service

## Overview
The Queue Timeout Background Service monitors queue items that have been allocated to robots but remain unprocessed for too long, and automatically deallocates them so they can be re-allocated.

## How It Works

The service runs periodically in the background and:

1. Queries for queue items that meet ALL of the following criteria:
   - `IsProcessed = false` (not yet processed)
   - `AllocatedRobotId != null` (allocated to a robot)
   - `UpdatedAt < (Current Time - QueueTimeoutMinutes)` (hasn't been updated recently)

2. For each timed-out queue item:
   - Sets `AllocatedRobotId = null` to deallocate the robot
   - Updates `UpdatedAt = Current Time` to track when the deallocation occurred
   - Logs a warning with the queue ID, robot ID, and timeout duration

3. After deallocating timed-out items, they become available for re-allocation by the `RpaAllocationBackgroundService`

## Configuration

The service is configured in `appsettings.json` under the `OrchestratorSettings` section:

```json
{
  "OrchestratorSettings": {
    "QueueTimeoutCheckIntervalSeconds": 60,
    "QueueTimeoutMinutes": 5
  }
}
```

### Configuration Parameters

- **QueueTimeoutCheckIntervalSeconds**: How often the service checks for timed-out queue items (default: 60 seconds)
- **QueueTimeoutMinutes**: Maximum time a queue item can remain allocated to a robot without being processed (default: 5 minutes)

## Logging

The service provides detailed logging:

- **Information level**: Service start/stop events and deallocation summaries
- **Warning level**: Individual queue item timeout details (queue ID, robot ID, timeout duration)
- **Error level**: Any exceptions during execution

## Example Logs

```
[Information] Queue Timeout Background Service started
[Warning] Queue 123 timed out after 5 minutes with robot 7. Deallocating robot.
[Information] Deallocated 1 timed-out queue items
```

## Integration with Other Services

This service works in conjunction with:

- **RpaAllocationBackgroundService**: After this service deallocates a queue item, the allocation service can pick it up again and assign it to an available robot
- **RpaAllocationBackgroundService robot timeout**: The allocation service already has robot timeout logic that marks robots as available if they don't respond. This queue timeout service complements that by also deallocating the queue items themselves.

## Implementation Details

- The service uses Entity Framework Core for database queries
- Runs in a background thread using .NET's `BackgroundService` base class
- Properly handles cancellation tokens for graceful shutdown
- Uses scoped services to ensure proper DbContext lifecycle management
- Includes exception handling to prevent service crashes
