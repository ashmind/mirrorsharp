import type { Connection } from '../protocol/connection';
import type { ContainerRoot } from './container-root';

export const installConnectionLossView = <O, U>(root: ContainerRoot, connection: Connection<O, U>) => {
    let messageElement: HTMLDivElement | undefined;
    const show = () => {
        if (!messageElement) {
            messageElement = document.createElement('div');
            messageElement.setAttribute('class', 'mirrorsharp-connection-loss-message');
            messageElement.innerText = 'Server connection lost, reconnecting…';
            root.element.appendChild(messageElement);
        }

        root.element.classList.add('mirrorsharp--connection-lost');
    };

    const hide = () => {
        root.element.classList.remove('mirrorsharp--connection-lost');
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