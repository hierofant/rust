import './ui/style.css';
import { GameData } from './core/gamedata';
import { Game } from './ui/game';

async function main() {
  const data = await GameData.fetch(import.meta.env.BASE_URL + 'data/');
  document.getElementById('loading')!.remove();
  const game = new Game(data, document.getElementById('view') as HTMLCanvasElement);
  (window as unknown as { game: Game }).game = game;
}

main().catch((e) => {
  document.getElementById('loading')!.textContent = `Failed to load: ${e}`;
  console.error(e);
});
