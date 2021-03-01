import execa from 'execa';
import { global } from './global-setup';
import { shouldSkipRender } from '../should-skip';

export default async () => {
    if (shouldSkipRender)
        return;

    const { chromeContainerId } = global;
    if (!chromeContainerId)
        return;
    await execa('docker', [
        'stop',
        chromeContainerId
    ]);
    console.log('Stopped Chrome container', chromeContainerId);
};