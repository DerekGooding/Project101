using Project1.Dungeon.Hazards;
using System;
using System.Collections.Generic;

namespace Project1.Dungeon.Puzzles;

public class PressurePlatePuzzle(Action onPuzzleSolved)
{
    private readonly List<PressurePlate> _plates = [];
    private readonly List<Point> _targetPositions = [];
    private readonly List<MovableBlock> _blocks = [];
    private readonly Action _onPuzzleSolved = onPuzzleSolved;
    private int _activatedPlates = 0;

    public void AddPlate(PressurePlate plate, Point targetPosition)
    {
        _plates.Add(plate);
        _targetPositions.Add(targetPosition);

        // Subscribe to plate events
        plate.OnActivate += () => CheckPlate(plate, true);
        plate.OnDeactivate += () => CheckPlate(plate, false);
    }

    public void AddBlock(MovableBlock block)
    {
        _blocks.Add(block);

        // Subscribe to block movement
        block.BlockMoved += OnBlockMoved;
    }

    private void OnBlockMoved(MovableBlock block, Point oldPosition, Point newPosition)
    {
        // Check if any plates need to be updated
        for (var i = 0; i < _plates.Count; i++)
        {
            var plate = _plates[i];

            // If a block moved off a plate
            if (oldPosition == plate.Position && newPosition != plate.Position)
            {
                plate.Deactivate();
            }

            // If a block moved onto a plate
            if (oldPosition != plate.Position && newPosition == plate.Position)
            {
                plate.Trigger(null); // Null player since the block is triggering it
            }
        }
    }

    private void CheckPlate(PressurePlate plate, bool isActivated)
    {
        if (isActivated)
        {
            _activatedPlates++;
        }
        else
        {
            _activatedPlates--;
        }

        // Check if all plates are activated
        if (_activatedPlates == _plates.Count)
        {
            _onPuzzleSolved?.Invoke();
        }
    }

    public void Reset()
    {
        _activatedPlates = 0;
        foreach (var block in _blocks)
        {
            block.Reset();
        }
    }
}
