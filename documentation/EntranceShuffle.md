# Entrance Shuffle

## Symmetric Shuffle
Understanding how symmetric shuffle picks related entires.

```
starting:
ins: [[[a0-l, a0-r], [a1-l, a1-r]], [[b0-l, b0-r], [b1-l, b1-r]], [[c0-l, c0-r], [c1-l, c1-r]]]
outs: [[[a0-l, a0-r], [a1-l, a1-r]], [[b0-l, b0-r], [b1-l, b1-r]], [[c0-l, c0-r], [c1-l, c1-r]]]

picked indexes:
1, 2

first left is shuffled, then the right out is matched in shuffling

from_items: [[b1-l, b1-r], [b0-l, b0-r], [c0-l, c0-r], [c1-l, c1-r]]
to_items: [[c0-l, c0-r], [c1-l, c1-r], [b1-l, b1-r], [b0-l, b0-r]]

left over:
ins: [[[a0-l, a0-r], [a1-l, a1-r]]]
outs: [[[a0-l, a0-r], [a1-l, a1-r]]]
```
