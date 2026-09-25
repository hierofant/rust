// Port of Rust.Global/ObjectWorkQueue.cs without the timing/telemetry parts.
// The game stops a run after a time budget (Stability.stabilityqueue = 9 ms); here the
// budget is a job count so runs are deterministic.
export class ObjectWorkQueue<T> {
  private queue: T[] = [];
  private head = 0;
  private containerTest = new Set<T>();

  constructor(
    private readonly runJob: (item: T) => void,
    private readonly shouldAdd: (item: T) => boolean = () => true,
    private readonly isValidToRun: (item: T) => boolean = (item) => item != null,
  ) {}

  get length() { return this.queue.length - this.head; }

  contains(item: T) { return this.containerTest.has(item); }

  add(item: T) {
    if (!this.contains(item) && this.shouldAdd(item)) {
      this.queue.push(item);
      this.containerTest.add(item);
    }
  }

  clear() {
    this.queue = [];
    this.head = 0;
    this.containerTest.clear();
  }

  /** Jobs added while running are processed in the same run, as in the original. */
  runQueue(maxJobs = Infinity) {
    let processed = 0;
    while (this.length > 0) {
      const item = this.queue[this.head++];
      this.containerTest.delete(item);
      if (this.isValidToRun(item)) this.runJob(item);
      if (++processed >= maxJobs) break;
    }
    if (this.head > 1024 && this.head * 2 > this.queue.length) {
      this.queue = this.queue.slice(this.head);
      this.head = 0;
    }
  }
}
