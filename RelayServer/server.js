// server.js
// Minimal WebSocket relay for the CarRacingControlsConnectivity
// remote-control system.
//
// It does NOT understand driving or garage messages — it just puts
// two connections (a "controller" phone and a "game" — WebGL build,
// PC build, or Android build) into the same numbered "room" and
// forwards whatever one side sends to the other, as-is.
//
// Protocol (JSON text frames):
//   → {"t":"join","room":"1234","role":"controller"|"game"}
//   ← {"t":"peer_joined"}   sent to both sides once the room has one of each role
//   ← {"t":"peer_left"}     sent to the remaining side when the other disconnects
//   any other message from one side is forwarded verbatim to the other side.
//
// If a new connection joins with the same room + role as one already
// connected (e.g. the game app moved from the Garage scene to a race
// scene and reconnected), the old connection is closed and replaced.
//
// Run:  node server.js
// Env:  PORT (default 8080)

const { WebSocketServer } = require('ws');

const PORT = process.env.PORT || 8080;
const wss = new WebSocketServer({ port: PORT });

// roomCode -> { controller: ws|null, game: ws|null }
const rooms = new Map();

function getRoom(code) {
  let room = rooms.get(code);
  if (!room) {
    room = { controller: null, game: null };
    rooms.set(code, room);
  }
  return room;
}

function otherRole(role) {
  return role === 'controller' ? 'game' : 'controller';
}

function send(ws, obj) {
  if (ws && ws.readyState === ws.OPEN) {
    ws.send(JSON.stringify(obj));
  }
}

function removeFromRoom(ws) {
  if (!ws.roomCode) return;
  const room = rooms.get(ws.roomCode);
  if (!room) return;

  if (room[ws.role] === ws) {
    room[ws.role] = null;
    send(room[otherRole(ws.role)], { t: 'peer_left' });
  }

  if (!room.controller && !room.game) {
    rooms.delete(ws.roomCode);
  }
}

wss.on('connection', (ws) => {
  ws.roomCode = null;
  ws.role = null;

  ws.on('message', (raw) => {
    let msg;
    try {
      msg = JSON.parse(raw);
    } catch {
      return; // ignore malformed frames
    }

    if (msg.t === 'join') {
      const room = getRoom(msg.room);
      const role = msg.role === 'game' ? 'game' : 'controller';

      // Replace any stale connection with the same room + role
      // (e.g. the game app reconnecting from a new scene).
      const stale = room[role];
      if (stale && stale !== ws) {
        stale.roomCode = null; // prevent its close handler from firing peer_left
        stale.close();
      }

      room[role] = ws;
      ws.roomCode = msg.room;
      ws.role = role;

      const peer = room[otherRole(role)];
      if (peer) {
        send(peer, { t: 'peer_joined' });
        send(ws, { t: 'peer_joined' });
      }
      return;
    }

    // Anything else: forward verbatim to the other side of the room.
    if (!ws.roomCode) return;
    const room = rooms.get(ws.roomCode);
    if (!room) return;
    const peer = room[otherRole(ws.role)];
    if (peer && peer.readyState === peer.OPEN) {
      peer.send(raw.toString());
    }
  });

  ws.on('close', () => removeFromRoom(ws));
  ws.on('error', () => removeFromRoom(ws));
});

console.log(`Relay server listening on ws://0.0.0.0:${PORT}`);
