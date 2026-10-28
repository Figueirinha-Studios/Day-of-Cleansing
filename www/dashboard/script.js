/* =====================================================================
   CONFIGURAÇÃO: preencha os campos abaixo
   ===================================================================== */
const CONFIG = {
  broker: {
    // O navegador só conecta via WebSocket (ws:// ou wss://), não na porta 1883.
    url: "wss://day-of-cleansing.feira-de-jogos.dev.br/mqtt",
    username: "",
    password: ""
  },

  // Segundos sem receber estado até a placa ser marcada como sem resposta
  staleAfterSeconds: 25,

  // Segundos que o botão fica "aguardando confirmação" depois de um clique
  pendingTimeoutSeconds: 15,

  boards: [
    {
      name: "esp32",
      stateTopic: "esp32/status",        // tópico onde a ESP32 publica o estado
      relays: [
        {
          id: "RELEA",                          // nome usado na mensagem de estado: RELEA=on
          name: "Relé A",
          commandTopic: "esp32/controller", // tópico onde a ESP32 recebe comandos
          onMessage:  "RELEAon",
          offMessage: "RELEAoff"
        },
        {
          id: "RELEB",
          name: "Relé B",
          commandTopic: "esp32/controller",
          onMessage:  "RELEBon",
          offMessage: "RELEBoff"
        }
      ]
    },
    {
      name: "arduino",
      stateTopic: "arduino/status", //onde o arduino publica
      relays: [
        {
          id: "RELEC",
          name: "Relé C",
          commandTopic: "arduino/controller", //onde o arduino le
          onMessage:  "on",
          offMessage: "off"
        }
      ]
    }
  ]
};
/* ===================================================================== */

const el = {
  conn: document.getElementById("conn"),
  connText: document.getElementById("connText"),
  notice: document.getElementById("notice"),
  boards: document.getElementById("boards")
};

// Estado em memória
CONFIG.boards.forEach(b => {
  b.lastSeen = null;
  b.relays.forEach(r => { r.state = null; r.pending = null; r.pendingTimer = null; });
});
let connected = false;

// Aviso quando ainda há valores por preencher
(function checkConfig() {
  const todo = [];
  if (/SEU_BROKER|PORTA/.test(CONFIG.broker.url)) todo.push("endereço do broker");
  const json = JSON.stringify(CONFIG.boards);
  if (/TOPICO_|MENSAGEM_/.test(json)) todo.push("tópicos e mensagens");
  if (todo.length) {
    el.notice.hidden = false;
    el.notice.innerHTML = "Falta preencher o <code>CONFIG</code> no início do script: " + todo.join(" e ") + ".";
  }
})();

/* ---------- Interface ---------- */
function build() {
  el.boards.innerHTML = "";
  CONFIG.boards.forEach((b, bi) => {
    const sec = document.createElement("section");
    sec.className = "board";
    sec.innerHTML =
      '<div class="board-head"><h2></h2><span class="seen" id="seen-' + bi + '"></span></div>' +
      '<div class="rack"></div>';
    sec.querySelector("h2").textContent = b.name;
    const rack = sec.querySelector(".rack");
    b.relays.forEach((r, ri) => {
      const slot = document.createElement("div");
      slot.className = "slot";
      slot.id = "slot-" + bi + "-" + ri;
      slot.innerHTML =
        '<button class="breaker" role="switch" aria-checked="false">' +
        '<span class="housing"><span class="lever"></span></span></button>' +
        '<span class="slot-name"></span><span class="slot-state"></span>';
      slot.querySelector(".slot-name").textContent = r.name;
      slot.querySelector("button").setAttribute("aria-label", r.name);
      slot.querySelector("button").addEventListener("click", () => toggle(b, r));
      rack.appendChild(slot);
    });
    el.boards.appendChild(sec);
  });
}

function isStale(b) {
  return b.lastSeen === null || (Date.now() - b.lastSeen) / 1000 > CONFIG.staleAfterSeconds;
}

function render() {
  CONFIG.boards.forEach((b, bi) => {
    const seen = document.getElementById("seen-" + bi);
    if (b.lastSeen === null) {
      seen.textContent = "Aguardando primeira leitura";
      seen.className = "seen";
    } else if (isStale(b)) {
      seen.textContent = "Sem resposta há " + Math.round((Date.now() - b.lastSeen) / 1000) + " s";
      seen.className = "seen stale";
    } else {
      seen.textContent = "Atualizado há " + Math.round((Date.now() - b.lastSeen) / 1000) + " s";
      seen.className = "seen";
    }
    b.relays.forEach((r, ri) => {
      const slot = document.getElementById("slot-" + bi + "-" + ri);
      const btn = slot.querySelector("button");
      const shown = r.pending !== null ? r.pending : r.state;      // o que a alavanca mostra
      slot.classList.toggle("is-on", shown === "on");
      slot.classList.toggle("pending", r.pending !== null);
      btn.setAttribute("aria-checked", shown === "on" ? "true" : "false");
      btn.disabled = !connected;
      let txt;
      if (r.pending !== null) txt = "Aguardando confirmação…";
      else if (r.state === null) txt = "Estado desconhecido";
      else txt = r.state === "on" ? "Ligado" : "Desligado";
      slot.querySelector(".slot-state").textContent = txt;
    });
  });
}

function setConn(kind, text) {
  el.conn.className = "conn " + kind;
  el.connText.textContent = text;
}

/* ---------- MQTT ---------- */
function toggle(board, relay) {
  if (!connected) return;
  const current = relay.pending !== null ? relay.pending : relay.state;
  const target = current === "on" ? "off" : "on";
  const message = target === "on" ? relay.onMessage : relay.offMessage;
  client.publish(relay.commandTopic, message, { qos: 1 }, err => {
    if (err) { console.error("Falha ao publicar:", err); relay.pending = null; render(); }
  });
  relay.pending = target;
  clearTimeout(relay.pendingTimer);
  relay.pendingTimer = setTimeout(() => { relay.pending = null; render(); },
                                  CONFIG.pendingTimeoutSeconds * 1000);
  render();
}

function handleState(board, text) {
  // Aceita "RELEA=on,RELEB=off" e também "esp32 RELEA=on,RELEB=off"
  const re = /([A-Za-z0-9_]+)\s*=\s*(on|off)/gi;
  let m, any = false;
  while ((m = re.exec(text)) !== null) {
    const relay = board.relays.find(r => r.id.toLowerCase() === m[1].toLowerCase());
    if (!relay) continue;
    relay.state = m[2].toLowerCase();
    relay.pending = null;
    clearTimeout(relay.pendingTimer);
    any = true;
  }
  if (any) board.lastSeen = Date.now();
}

const client = mqtt.connect(CONFIG.broker.url, {
  username: CONFIG.broker.username || undefined,
  password: CONFIG.broker.password || undefined,
  clean: true,
  reconnectPeriod: 3000,
  connectTimeout: 8000
});

client.on("connect", () => {
  connected = true;
  setConn("ok", "Conectado ao broker");
  const topics = [...new Set(CONFIG.boards.map(b => b.stateTopic))];
  client.subscribe(topics, err => { if (err) console.error("Falha ao assinar:", err); });
  render();
});
client.on("reconnect", () => { connected = false; setConn("", "Reconectando…"); render(); });
client.on("close", () => { connected = false; setConn("bad", "Sem conexão com o broker"); render(); });
client.on("error", err => { console.error("MQTT:", err); setConn("bad", "Erro de conexão"); });

client.on("message", (topic, payload) => {
  const text = payload.toString();
  CONFIG.boards.filter(b => b.stateTopic === topic).forEach(b => handleState(b, text));
  render();
});

build();
render();
setInterval(render, 1000);   // atualiza "há X s" e o aviso de falta de resposta
