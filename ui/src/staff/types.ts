export type RoomState = 'Ready' | 'Occupied' | 'Dirty'

export type ImportState = 'init' | 'processing' | 'error' | 'processing-completed' | 'importing' | 'importing-completed'

export interface Room {
    number: string;
    state: RoomState;
}

export interface CompletedProcessingProps {
    state: ImportState,
    rooms: Room[],
    errors: string[],
    onSubmit: () => void,
    onCancel: () => void
}