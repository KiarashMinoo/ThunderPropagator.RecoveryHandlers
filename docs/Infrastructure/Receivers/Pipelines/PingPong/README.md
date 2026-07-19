# PingPong Pipeline

## Overview
Health check endpoint that responds with "Pong" to verify connection and channel availability.

## Files (2 files, 75 LOC)

| File | LOC | Description |
|------|-----|-------------|
| PingPongReceivePipeline.cs | 62 | Ping request pipeline |
| PingPongReceivePipelineResponseDto.cs | 13 | Pong response DTO |

## Request/Response

**Request**:
```json
{
  "RequestKey": "Ping"
}
```

**Response**:
```json
{
  "Message": "Pong",
  "Timestamp": "2025-12-28T10:30:00Z"
}
```

## Use Cases

1. **Connection Health**: Verify WebSocket/MQTT connection is alive
2. **Keepalive**: Prevent connection timeout
3. **Latency Measurement**: Calculate round-trip time
4. **Load Balancer Health Checks**: Verify backend availability

## Examples

### JavaScript Client with Keepalive

```javascript
class WebSocketClient {
    constructor(url) {
        this.ws = new WebSocket(url);
        this.startPingInterval();
    }
    
    startPingInterval() {
        setInterval(() => {
            const pingStart = Date.now();
            
            this.ws.send(JSON.stringify({ RequestKey: "Ping" }));
            
            this.ws.addEventListener('message', (event) => {
                const response = JSON.parse(event.data);
                if (response.Message === "Pong") {
                    const latency = Date.now() - pingStart;
                    console.log(`Latency: ${latency}ms`);
                }
            }, { once: true });
        }, 30000); // Every 30 seconds
    }
}
```

## See Also
- [Health Checks](../../../../../Application/HealthChecks/README.md)
