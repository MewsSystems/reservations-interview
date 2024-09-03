import { useRef, useState } from 'react';
import Papa from 'papaparse';
import { Box, Button, Text } from "@radix-ui/themes";
import { validateRoomNumber } from '../../utils/validation';
import { CompletedProcessingProps, ImportState, Room, RoomState } from '../types';
import { postRooms } from '../api';
import { showSuccessToast, useShowInfoToast } from '../../utils/toasts';

export const ErrorMessages = ({ errors, title }: { title: string, errors: string[] }) => (
    <Box style={{ maxHeight: 800, overflow: 'auto' }}>
        <Text size="6" weight="bold" style={{ color: '#a94442' }}>{title}</Text>
        <ul style={{ listStyleType: 'none', padding: 0 }}>
            {errors.map((error, index) => (
                <li key={index} style={{ paddingBottom: '5px' }}>
                    <Text >{error}</Text>
                </li>
            ))}
        </ul>
    </Box>
);

const CompletedProcessing = ({ state, errors, rooms, onSubmit, onCancel }: CompletedProcessingProps) => {
    return (
        <Box style={{ display: 'flex', flexDirection: 'row', justifyContent: 'center', alignItems: 'center', paddingTop: '30px' }}>
            {errors && errors.length > 0 && <ErrorMessages title="ValidationErrors" errors={errors}></ErrorMessages>}
            <Box style={{ display: "flex", flexDirection: "column", width: "30%", textAlign: 'center' }}>
                <Box style={{ paddingBottom: '30px' }}>
                    {state === 'importing' ?
                        <Text style={{ textAlign: 'center' }}>{`Importing ${rooms.length} rooms...`} </Text> :
                        state === 'importing-completed' ?
                            <Text style={{ textAlign: 'center' }}>{`Importing completed. Failed: ${errors.length} room imports...`} </Text> :
                            <Text style={{ textAlign: 'center' }}>{`Done processing. Successfully processed ${rooms.length} rooms...`} </Text>
                    }
                </Box>
                <Box style={{ justifyContent: 'center', alignItems: 'center' }}>
                    <Button style={{ marginRight: '10px' }}
                        onClick={onSubmit}
                        disabled={rooms.length == 0 || state == 'importing'}>{state == 'importing' ? 'Importing...' : 'Import'}</Button>
                    <Button onClick={onCancel}>Cancel</Button>
                </Box>
            </Box>
        </Box >
    )
}

const ImportProcessing = () => {
    return <Box><Text>Processing...</Text></Box>
}

export const ImportRooms = () => {
    const [errors, setErrors] = useState<string[]>([]);
    const [importState, setImportState] = useState<ImportState>('init');
    const [rooms, setRooms] = useState<Room[]>([]);
    const fileInputRef = useRef<HTMLInputElement | null>(null);
    const apiCallFailToast = useShowInfoToast("Import failed")
    function onCancel() {
        setImportState('init')
        setRooms([])
    }

    function onSubmit() {
        setImportState('importing')
        postRooms(rooms)
            .then((x) => {
                if (x.fail != null && x.fail.length > 0) {
                    setImportState('error')
                    setRooms(x.fail.map((item) => ({
                        number: item.room.number,
                        state: item.room.state,
                    }) as Room))
                    setErrors(x.fail.map(x => `Room ${x.room.number} (${x.room.state}). Error: ${x.errorMessage}`))
                }
                if (x.success && x.success.length > 0)
                    showSuccessToast(`Successfuly imported ${x.success.length} rooms...`)
            }).catch(() => {
                apiCallFailToast()
            }).then(() => {
                setRooms([])
                setImportState('importing-completed')
            })
    }

    const handleDrop = (files: FileList) => {
        if (files.length > 1) {
            setImportState('error')
            setErrors(["Cannot import more than one file!"]);
            return;
        }
        const file = files[0];
        if (file.type !== 'text/csv') {
            setImportState('error')
            setErrors(["Invalid file type! Only CSV files are supported."]);
            return;
        }
        processFile(file)
    }

    const processFile = (file: File) => {
        if (!file) return;

        setImportState('processing')

        Papa.parse<Room>(file, {
            header: true,
            skipEmptyLines: true,
            complete: (results) => {
                const parsedRooms = results.data;
                const validationResult = validateRooms(parsedRooms);

                setImportState('processing-completed')
                if (validationResult.errors.length > 0) {
                    setImportState('error')
                    setErrors(validationResult.errors);
                }

                setRooms(validationResult.validRooms);
            },
            error() {
                setImportState('error')
                setErrors(['Unexpected error while importing CSV file...']);
            },
        });
    };

    type ValidateRoomType = {
        errors: string[],
        validRooms: Room[]
    }

    const validateRooms = (rooms: Room[]): ValidateRoomType => {
        const errors: string[] = [];
        const validRooms: Room[] = [];
        if (rooms.length > 500) {
            errors.push("You can only import up to 500 rooms.");
        }

        const isValidRoomState = (state: any): state is RoomState => {
            const validStates: RoomState[] = ['Ready', 'Occupied', 'Dirty'];
            return validStates.includes(state);
        };

        rooms.forEach((room, index) => {
            if (!isValidRoomState(room?.state)) {
                errors.push(`Row ${index + 1}: Invalid room state: '${room.state}'. Use values ['Ready', 'Occupied', 'Dirty']...`);
                return;
            }
            const roomErrors = validateRoomNumber(room?.number);
            if (roomErrors && roomErrors.length > 0) {
                const prefixedErrors = roomErrors.map(error => `Row ${index + 1}: Invalid Room number ${room?.number}. ${error}`);
                errors.push(...prefixedErrors)
                return;
            }
            validRooms.push(room);
        });
        return { errors, validRooms };
    };

    return (
        importState === 'processing' ? (
            <ImportProcessing />
        ) : (
            importState === 'processing-completed' || importState === 'error' || importState == 'importing' || importState === 'importing-completed' ? (
                <CompletedProcessing
                    state={importState}
                    rooms={rooms}
                    errors={errors}
                    onSubmit={onSubmit}
                    onCancel={onCancel}
                />
            ) : (
                <Box style={{ display: 'flex', flex: 1, justifyContent: 'center', alignItems: 'center' }}>
                    <div
                        onDrop={e => {
                            e.preventDefault();
                            e.stopPropagation();
                            handleDrop(e.dataTransfer.files)
                        }}
                        onDragOver={(e) => {
                            e.preventDefault();
                            e.stopPropagation();
                        }}
                        onClick={() => fileInputRef.current?.click()}
                        style={{
                            border: '2px dashed #ccc',
                            textAlign: 'center',
                            width: '50%',
                            height: 150,
                            display: 'flex',
                            justifyContent: 'center',
                            alignItems: 'center',
                            cursor: 'pointer'
                        }}
                    >
                        <Text style={{ height: '20px' }}>Drag and drop your CSV file here or click to select a file</Text>
                        <input
                            type="file"
                            ref={fileInputRef}
                            accept=".csv"
                            onChange={(e) => {
                                e.preventDefault();
                                e.stopPropagation();
                                if (e.target.files) {
                                    handleDrop(e.target.files);
                                }
                            }}
                            style={{ display: 'none' }}
                        />
                    </div>
                </Box>
            )
        )
    );
};
