const WebSocket = require('ws');
const fs = require('fs');

const PORT = 8080;
const LOG_FILE = 'mensagens.txt';

// ==========================================
// CONEXÕES
// ==========================================

// Apenas UMA Unity pode estar conectada
let unity = null;

// Vários PCs podem estar conectados
// "pc1" -> WebSocket
// "pc2" -> WebSocket
// "pc3" -> WebSocket
const pcs = new Map();

const wss = new WebSocket.Server({ port: PORT });


// ==========================================
// LOG
// ==========================================

function log(mensagem) {
    const dataHora = new Date().toLocaleString();

    console.log(`[${dataHora}] ${mensagem}`);

    fs.appendFileSync(
        LOG_FILE,
        `[${dataHora}] ${mensagem}\n`
    );
}


// ==========================================
// ENCONTRA O MENOR ID DE PC DISPONÍVEL
// ==========================================

function encontrarIdPC() {

    let numero = 1;

    while (pcs.has(`pc${numero}`)) {
        numero++;
    }

    return `pc${numero}`;
}


// ==========================================
// NOVA CONEXÃO
// ==========================================

wss.on('connection', (ws, req) => {

    console.log(
        'Nova conexão:',
        req.socket.remoteAddress
    );

    // Ainda não identificado
    ws.clientType = null;
    ws.clientId = null;


    // ==========================================
    // RECEBIMENTO DE MENSAGENS
    // ==========================================

    ws.on('message', (message) => {

        const mensagemString = message.toString();

        try {

            const dados = JSON.parse(mensagemString);


            // ==========================================
            // IDENTIFICAÇÃO
            // ==========================================

            if (dados.type === 'identify') {

                const tipo = dados.client;


                // ==========================================
                // UNITY
                // ==========================================

                if (tipo === 'unity') {

                    // Já existe uma Unity
                    if (
                        unity !== null &&
                        unity.readyState === WebSocket.OPEN
                    ) {

                        log(
                            'Tentativa de conexão de uma segunda Unity. Nova conexão rejeitada.'
                        );

                        ws.send(JSON.stringify({
                            type: 'identify_rejected',
                            reason: 'unity_already_connected'
                        }));

                        ws.close();

                        return;
                    }


                    // Registra a Unity
                    unity = ws;

                    ws.clientType = 'unity';
                    ws.clientId = 'unity';


                    // Confirma para a Unity
                    ws.send(JSON.stringify({
                        type: 'identify_success',
                        client: 'unity'
                    }));


                    log('Unity conectada.');

                    return;
                }


                // ==========================================
                // PC
                // ==========================================

                if (tipo === 'pc') {

                    // Encontra primeiro ID livre
                    const clientId = encontrarIdPC();


                    // Registra o PC
                    pcs.set(clientId, ws);

                    ws.clientType = 'pc';
                    ws.clientId = clientId;


                    // Informa ao PC qual é sua identidade
                    ws.send(JSON.stringify({
                        type: 'identify_success',
                        client: clientId
                    }));


                    log(`PC conectado como ${clientId}.`);

                    return;
                }


                // ==========================================
                // TIPO INVÁLIDO
                // ==========================================

                log(
                    `Cliente tentou se identificar com tipo inválido: ${tipo}`
                );

                ws.send(JSON.stringify({
                    type: 'identify_rejected',
                    reason: 'invalid_client_type'
                }));

                ws.close();

                return;
            }


            // ==========================================
            // CLIENTE AINDA NÃO IDENTIFICADO
            // ==========================================

            if (!ws.clientType) {

                log(
                    'Mensagem ignorada: cliente ainda não identificado.'
                );

                return;
            }


            // ==========================================
            // LOG DA MENSAGEM
            // ==========================================

            log(
                `[${ws.clientId}] ${mensagemString}`
            );


            // ==========================================
            // UNITY -> TODOS OS PCS
            // ==========================================

            if (ws.clientType === 'unity') {

                let quantidadeEnviada = 0;


                for (const [pcId, pc] of pcs) {

                    if (pc.readyState === WebSocket.OPEN) {

                        pc.send(mensagemString);

                        quantidadeEnviada++;
                    }
                }


                log(
                    `Unity -> ${quantidadeEnviada} PC(s)`
                );

                return;
            }


            // ==========================================
            // PC -> UNITY
            // ==========================================

            if (ws.clientType === 'pc') {

                if (
                    unity !== null &&
                    unity.readyState === WebSocket.OPEN
                ) {

                    unity.send(mensagemString);

                    log(
                        `${ws.clientId} -> Unity`
                    );

                } else {

                    log(
                        `${ws.clientId} tentou enviar mensagem, mas a Unity não está conectada.`
                    );
                }

                return;
            }

        }

        catch (err) {

            log(
                `Mensagem inválida de ${ws.clientId || 'cliente desconhecido'}: ${mensagemString}`
            );

        }

    });


    // ==========================================
    // DESCONEXÃO
    // ==========================================

    ws.on('close', () => {


        // ==========================================
        // UNITY DESCONECTOU
        // ==========================================

        if (ws.clientType === 'unity') {

            // Só remove se essa ainda for
            // a Unity registrada
            if (unity === ws) {

                unity = null;

                log('Unity desconectou.');
            }

            return;
        }


        // ==========================================
        // PC DESCONECTOU
        // ==========================================

        if (ws.clientType === 'pc') {

            // Só remove se essa conexão
            // ainda estiver registrada
            if (pcs.get(ws.clientId) === ws) {

                pcs.delete(ws.clientId);

                log(
                    `${ws.clientId} desconectou. ID liberado.`
                );
            }

            return;
        }


        // ==========================================
        // NÃO IDENTIFICADO
        // ==========================================

        log(
            'Cliente não identificado desconectou.'
        );

    });


    // ==========================================
    // ERRO
    // ==========================================

    ws.on('error', (err) => {

        log(
            `Erro em ${ws.clientId || 'cliente desconhecido'}: ${err.message}`
        );

    });

});


// ==========================================
// SERVIDOR
// ==========================================

console.log(
    `WebSocket rodando na porta ${PORT}`
);
