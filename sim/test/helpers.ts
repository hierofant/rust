import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { GameData } from '../src/core/gamedata';

let cached: GameData | null = null;
export function loadData(): GameData {
  if (cached) return cached;
  const json = JSON.parse(readFileSync(join(__dirname, '../public/data/game.json'), 'utf8'));
  const bin = readFileSync(join(__dirname, '../public/data/meshes.bin'));
  cached = new GameData(json);
  cached.meshBuffer = bin.buffer.slice(bin.byteOffset, bin.byteOffset + bin.byteLength);
  return cached;
}
