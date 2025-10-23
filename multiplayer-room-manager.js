// Multiplayer Room Manager for BeanBox
// Handles room creation, joining, and gravity synchronization

class MultiplayerRoomManager {
    constructor() {
        this.baseUrl = 'https://beanbox.netlify.app/.netlify/functions';
        this.currentRoom = null;
        this.isHost = false;
        this.players = new Map();
        this.gravityData = new Map();
        this.updateInterval = null;
        this.isConnected = false;
    }

    /**
     * Create a new room
     * @param {string} roomCode - 6-character room code
     * @returns {Promise<Object>} Room data
     */
    async createRoom(roomCode) {
        try {
            const response = await fetch(`${this.baseUrl}/room-manager`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ roomCode })
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error || 'Failed to create room');
            }

            const roomData = await response.json();
            this.currentRoom = roomData;
            this.isHost = true;
            this.isConnected = true;

            console.log(`Room created: ${roomCode}`);
            return roomData;
        } catch (error) {
            console.error('Error creating room:', error);
            throw error;
        }
    }

    /**
     * Join an existing room
     * @param {string} roomCode - 6-character room code
     * @returns {Promise<Object>} Room data
     */
    async joinRoom(roomCode) {
        try {
            const response = await fetch(`${this.baseUrl}/room-manager?roomCode=${roomCode}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error || 'Failed to join room');
            }

            const roomData = await response.json();
            this.currentRoom = roomData;
            this.isHost = false;
            this.isConnected = true;

            console.log(`Joined room: ${roomCode}`);
            return roomData;
        } catch (error) {
            console.error('Error joining room:', error);
            throw error;
        }
    }

    /**
     * Leave the current room
     */
    leaveRoom() {
        this.currentRoom = null;
        this.isHost = false;
        this.isConnected = false;
        this.players.clear();
        this.gravityData.clear();
        
        if (this.updateInterval) {
            clearInterval(this.updateInterval);
            this.updateInterval = null;
        }

        console.log('Left room');
    }

    /**
     * Update player gravity data
     * @param {Vector3} gravityDirection - Gravity direction vector
     */
    updatePlayerGravity(gravityDirection) {
        if (!this.isConnected) return;

        const playerId = this.getPlayerId();
        this.gravityData.set(playerId, {
            direction: gravityDirection,
            timestamp: Date.now()
        });

        // Send to Unity if available
        this.sendGravityToUnity(gravityDirection);
    }

    /**
     * Send gravity data to Unity
     * @param {Vector3} gravityDirection - Gravity direction vector
     */
    sendGravityToUnity(gravityDirection) {
        if (typeof gameInstance !== 'undefined' && gameInstance.SendMessage) {
            const gravityString = `${gravityDirection.x},${gravityDirection.y},${gravityDirection.z}`;
            gameInstance.SendMessage('MultiplayerGravityController', 'SetMotionDataString', gravityString);
        }
    }

    /**
     * Get unique player ID
     * @returns {string} Player ID
     */
    getPlayerId() {
        // Use device fingerprint or generate unique ID
        if (!this.playerId) {
            this.playerId = 'player_' + Math.random().toString(36).substr(2, 9);
        }
        return this.playerId;
    }

    /**
     * Check if gravity is aligned between players
     * @param {number} threshold - Alignment threshold in degrees
     * @returns {boolean} True if aligned
     */
    isGravityAligned(threshold = 30) {
        if (this.gravityData.size < 2) return false;

        const directions = Array.from(this.gravityData.values()).map(data => data.direction);
        const averageDirection = this.calculateAverageDirection(directions);

        for (const direction of directions) {
            const angle = this.calculateAngle(direction, averageDirection);
            if (angle > threshold) {
                return false;
            }
        }

        return true;
    }

    /**
     * Calculate average direction from array of directions
     * @param {Vector3[]} directions - Array of direction vectors
     * @returns {Vector3} Average direction
     */
    calculateAverageDirection(directions) {
        const sum = directions.reduce((acc, dir) => ({
            x: acc.x + dir.x,
            y: acc.y + dir.y,
            z: acc.z + dir.z
        }), { x: 0, y: 0, z: 0 });

        const length = directions.length;
        return {
            x: sum.x / length,
            y: sum.y / length,
            z: sum.z / length
        };
    }

    /**
     * Calculate angle between two vectors
     * @param {Vector3} a - First vector
     * @param {Vector3} b - Second vector
     * @returns {number} Angle in degrees
     */
    calculateAngle(a, b) {
        const dot = a.x * b.x + a.y * b.y + a.z * b.z;
        const magA = Math.sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
        const magB = Math.sqrt(b.x * b.x + b.y * b.y + b.z * b.z);
        const cos = dot / (magA * magB);
        return Math.acos(Math.max(-1, Math.min(1, cos))) * (180 / Math.PI);
    }

    /**
     * Get current room code
     * @returns {string|null} Room code or null
     */
    getRoomCode() {
        return this.currentRoom ? this.currentRoom.roomCode : null;
    }

    /**
     * Check if currently in a room
     * @returns {boolean} True if in room
     */
    isInRoom() {
        return this.isConnected && this.currentRoom !== null;
    }

    /**
     * Check if currently hosting
     * @returns {boolean} True if hosting
     */
    isHosting() {
        return this.isHost;
    }

    /**
     * Get number of players in room
     * @returns {number} Player count
     */
    getPlayerCount() {
        return this.gravityData.size;
    }
}

// Global instance
window.multiplayerRoomManager = new MultiplayerRoomManager();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = MultiplayerRoomManager;
}
