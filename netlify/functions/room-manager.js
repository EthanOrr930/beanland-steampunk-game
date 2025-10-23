// Netlify Function for room management
exports.handler = async (event, context) => {
  // Enable CORS
  const headers = {
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Headers': 'Content-Type',
    'Access-Control-Allow-Methods': 'GET, POST, OPTIONS'
  };

  // Handle preflight requests
  if (event.httpMethod === 'OPTIONS') {
    return {
      statusCode: 200,
      headers,
      body: ''
    };
  }

  try {
    const { httpMethod, body } = event;
    const data = body ? JSON.parse(body) : {};

    switch (httpMethod) {
      case 'GET':
        return handleGetRoom(data, headers);
      case 'POST':
        return handleCreateRoom(data, headers);
      default:
        return {
          statusCode: 405,
          headers,
          body: JSON.stringify({ error: 'Method not allowed' })
        };
    }
  } catch (error) {
    console.error('Error:', error);
    return {
      statusCode: 500,
      headers,
      body: JSON.stringify({ error: 'Internal server error' })
    };
  }
};

// Simple in-memory room storage (in production, use a database)
const rooms = new Map();

function handleGetRoom(data, headers) {
  const { roomCode } = data;
  
  if (!roomCode) {
    return {
      statusCode: 400,
      headers,
      body: JSON.stringify({ error: 'Room code required' })
    };
  }

  const room = rooms.get(roomCode);
  if (!room) {
    return {
      statusCode: 404,
      headers,
      body: JSON.stringify({ error: 'Room not found' })
    };
  }

  return {
    statusCode: 200,
    headers,
    body: JSON.stringify({
      roomCode: room.roomCode,
      players: room.players,
      createdAt: room.createdAt,
      isActive: room.isActive
    })
  };
}

function handleCreateRoom(data, headers) {
  const { roomCode } = data;
  
  if (!roomCode) {
    return {
      statusCode: 400,
      headers,
      body: JSON.stringify({ error: 'Room code required' })
    };
  }

  // Check if room already exists
  if (rooms.has(roomCode)) {
    return {
      statusCode: 409,
      headers,
      body: JSON.stringify({ error: 'Room already exists' })
    };
  }

  // Create new room
  const room = {
    roomCode,
    players: [],
    createdAt: new Date().toISOString(),
    isActive: true
  };

  rooms.set(roomCode, room);

  // Clean up old rooms (older than 30 minutes)
  const thirtyMinutesAgo = new Date(Date.now() - 30 * 60 * 1000);
  for (const [code, roomData] of rooms.entries()) {
    if (new Date(roomData.createdAt) < thirtyMinutesAgo) {
      rooms.delete(code);
    }
  }

  return {
    statusCode: 201,
    headers,
    body: JSON.stringify({
      roomCode: room.roomCode,
      players: room.players,
      createdAt: room.createdAt,
      isActive: room.isActive
    })
  };
}
