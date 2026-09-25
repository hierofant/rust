import './ui/style.css';
import { GameData } from './core/gamedata';
import { Game } from './ui/game';
import { Visuals } from './ui/visual';

async function main() {
  const [data, visuals] = await Promise.all([
    GameData.fetch(import.meta.env.BASE_URL + 'data/'),
    Visuals.load(import.meta.env.BASE_URL + 'visual/'),
  ]);
  document.getElementById('loading')!.remove();
  const game = new Game(data, document.getElementById('view') as HTMLCanvasElement, visuals);
  (window as unknown as { game: Game }).game = game;
}

main().catch((e) => {
  document.getElementById('loading')!.textContent = `Failed to load: ${e}`;
  console.error(e);
});
