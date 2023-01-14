import type { Connection } from '../protocol/connection';

export const connectionLossView = <O, U>(container: HTMLElement, connection: Connection<O, U>) => {
    let messageElement: HTMLDivElement | undefined;
    const show = () => {
        if (!messageElement) {
            messageElement = document.createElement('div');
            messageElement.setAttribute('class', 'mirrorsharp-connection-loss-message');
            messageElement.innerText = 'Server connection lost, reconnecting…';
            container.appendChild(messageElement);
        }

        container.classList.add('mirrorsharp--connection-lost');
    };

    const hide = () => {
        container.classList.remove('mirrorsharp--connection-lost');
    };

    const removeConnectionListeners = connection.addEventListeners({
        open: () => hide(),
        close: () => show()
    });

    return () => {
        removeConnectionListeners();
        messageElement?.remove();
    };
};