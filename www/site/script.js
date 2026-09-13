/*
 * DAY OF CLEANSING
 * Controle de Megafones
 *
 * O site envia:
 * {"type":"megafone1"}
 * {"type":"megafone2"}
 * ...
 * {"type":"megafone6"}
 */

// ============================================================
// CONFIGURAÇÃO
// ============================================================

// Coloque aqui o endereço do seu servidor WebSocket.
//
// Exemplos:
// ws://IP_DO_SERVIDOR:PORTA
// wss://seu-dominio.com
//
// Se a página estiver em HTTPS, prefira WSS.
const WS_URL = "wss://day-of-cleansing.feira-de-jogos.dev.br/websocket/";

// Tempo entre tentativas automáticas de reconexão.
const RECONNECT_DELAY = 3000;

// ============================================================
// WEBSOCKET
// ============================================================

let socket = null;
let reconnectTimer = null;

const statusElement = document.getElementById("connectionStatus");
const statusText = document.getElementById("statusText");
const notification = document.getElementById("notification");

function setConnectionStatus(connected) {
    statusElement.classList.toggle("connected", connected);
    statusElement.classList.toggle("disconnected", !connected);
    statusText.textContent = connected ? "Conectado" : "Desconectado";
}

function connectWebSocket() {
    if (socket && (
        socket.readyState === WebSocket.OPEN ||
        socket.readyState === WebSocket.CONNECTING
    )) {
        return;
    }

    try {
        socket = new WebSocket(WS_URL);
    } catch (error) {
        console.error("Erro ao criar WebSocket:", error);
        setConnectionStatus(false);
        scheduleReconnect();
        return;
    }

    socket.addEventListener("open", () => {
        console.log("WebSocket conectado.");

        // Identifica esta conexão como o PC para o servidor.
        const identification = {
            type: "identify",
            client: "pc"
        };

        socket.send(JSON.stringify(identification));
        console.log("Identificação enviada:", JSON.stringify(identification));

        setConnectionStatus(true);
        showNotification("Conectado ao servidor.");
    });

    socket.addEventListener("message", (event) => {
        // Reservado para mensagens que o servidor possa mandar futuramente.
        console.log("Mensagem recebida:", event.data);
    });

    socket.addEventListener("close", () => {
        console.log("WebSocket desconectado.");
        setConnectionStatus(false);
        scheduleReconnect();
    });

    socket.addEventListener("error", (error) => {
        console.error("Erro no WebSocket:", error);
        setConnectionStatus(false);
    });
}

function scheduleReconnect() {
    if (reconnectTimer !== null) {
        return;
    }

    reconnectTimer = setTimeout(() => {
        reconnectTimer = null;
        connectWebSocket();
    }, RECONNECT_DELAY);
}

// ============================================================
// PALCO DO MAPA
// ============================================================
//
// A imagem do mapa usa object-fit: contain, então ela pode não
// preencher 100% do container (sobra espaço vazio nas laterais
// ou em cima/embaixo, dependendo da proporção da tela).
//
// Para os botões (posicionados por %) ficarem sempre alinhados
// com a imagem, calculamos aqui a área exata que a imagem ocupa
// e aplicamos esse tamanho/posição ao #mapStage, que é o elemento
// em relação ao qual os botões são posicionados.

const mapElement = document.getElementById("map");
const mapImage = document.getElementById("mapImage");
const mapStage = document.getElementById("mapStage");

function updateMapStage() {
    if (!mapImage.naturalWidth || !mapImage.naturalHeight) {
        return;
    }

    const containerWidth = mapElement.clientWidth;
    const containerHeight = mapElement.clientHeight;

    if (containerWidth === 0 || containerHeight === 0) {
        return;
    }

    const imageRatio = mapImage.naturalWidth / mapImage.naturalHeight;
    const containerRatio = containerWidth / containerHeight;

    let width;
    let height;

    if (containerRatio > imageRatio) {
        // Container mais "largo" que a imagem: sobra espaço nas laterais.
        height = containerHeight;
        width = height * imageRatio;
    } else {
        // Container mais "alto" que a imagem: sobra espaço em cima/embaixo.
        width = containerWidth;
        height = width / imageRatio;
    }

    const left = (containerWidth - width) / 2;
    const top = (containerHeight - height) / 2;

    mapStage.style.width = `${width}px`;
    mapStage.style.height = `${height}px`;
    mapStage.style.left = `${left}px`;
    mapStage.style.top = `${top}px`;
}

if (mapImage.complete) {
    updateMapStage();
} else {
    mapImage.addEventListener("load", updateMapStage);
}

// Em vez de confiar apenas nos eventos "resize"/ResizeObserver (que nem
// sempre disparam de forma confiável — por exemplo ao redimensionar pela
// ferramenta de dispositivo do navegador, ou em certas trocas de
// orientação no celular), fica-se verificando a cada frame se o tamanho
// do container mudou. Isso garante que o palco (e os botões dentro dele)
// sempre acompanhem o tamanho real do mapa, não importa o motivo da
// mudança.
let lastMapWidth = -1;
let lastMapHeight = -1;

function watchMapSize() {
    const width = mapElement.clientWidth;
    const height = mapElement.clientHeight;

    if (width !== lastMapWidth || height !== lastMapHeight) {
        lastMapWidth = width;
        lastMapHeight = height;
        updateMapStage();
    }

    requestAnimationFrame(watchMapSize);
}

requestAnimationFrame(watchMapSize);

// ============================================================
// MEGAFONES
// ============================================================

function sendMegaphone(number) {
    const message = {
        type: `megafone${number}`
    };

    if (!socket || socket.readyState !== WebSocket.OPEN) {
        console.warn("WebSocket não está conectado.");
        showNotification("Sem conexão com o servidor.", true);
        return;
    }

    const json = JSON.stringify(message);

    console.log("Enviando:", json);
    socket.send(json);

    activateButtons(number);
    showNotification(`Megafone ${number} acionado.`);
}

function activateButtons(number) {
    const buttons = document.querySelectorAll(
        `[data-megaphone="${number}"]`
    );

    buttons.forEach(button => {
        button.classList.add("active");

        setTimeout(() => {
            button.classList.remove("active");
        }, 350);
    });
}

document.querySelectorAll("[data-megaphone]").forEach(button => {
    button.addEventListener("click", () => {
        const number = Number(button.dataset.megaphone);
        sendMegaphone(number);
    });
});

// ============================================================
// NOTIFICAÇÃO
// ============================================================

let notificationTimer = null;

function showNotification(message, isError = false) {
    notification.textContent = message;
    notification.classList.toggle("error", isError);
    notification.classList.add("show");

    clearTimeout(notificationTimer);

    notificationTimer = setTimeout(() => {
        notification.classList.remove("show");
    }, 1800);
}

// ============================================================
// INICIALIZAÇÃO
// ============================================================

setConnectionStatus(false);
connectWebSocket();
