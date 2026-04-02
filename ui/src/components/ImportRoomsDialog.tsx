import { useState, useRef, useCallback } from "react";
import { Box, Button, Dialog, Flex, Text, Badge } from "@radix-ui/themes";
import { importRoomsCsv, type ImportResult } from "../reservations/api";
import { handleApiError, showSuccessToast } from "../utils/toasts";

interface ImportRoomsDialogProps {
  open: boolean;
  onClose: () => void;
  onImported: () => void;
}

type Phase = "pick" | "uploading" | "done";

export function ImportRoomsDialog({
  open,
  onClose,
  onImported,
}: ImportRoomsDialogProps) {
  const [file, setFile] = useState<File | null>(null);
  const [phase, setPhase] = useState<Phase>("pick");
  const [result, setResult] = useState<ImportResult | null>(null);
  const [dragOver, setDragOver] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  const reset = useCallback(() => {
    setFile(null);
    setPhase("pick");
    setResult(null);
    setDragOver(false);
  }, []);

  function handleClose() {
    reset();
    onClose();
  }

  const MAX_FILE_SIZE = 102_400; // 100 KB — matches server limit

  function handleFile(f: File | undefined) {
    if (!f) return;
    if (!f.name.endsWith(".csv")) return;
    setFile(f);
  }

  async function handleUpload() {
    if (!file) return;
    setPhase("uploading");
    try {
      const res = await importRoomsCsv(file);
      setResult(res);
      setPhase("done");
      if (res.imported > 0) {
        showSuccessToast(`Imported ${res.imported} room${res.imported !== 1 ? "s" : ""}.`);
        onImported();
      }
    } catch (err) {
      await handleApiError(err, "Failed to import rooms.");
      setPhase("pick");
    }
  }

  return (
    <Dialog.Root open={open} onOpenChange={(o) => { if (!o) handleClose(); }}>
      <Dialog.Content maxWidth="520px">
        <Dialog.Title>Import Rooms from CSV</Dialog.Title>

        {phase === "pick" && (
          <>
            <Text size="2" as="p" mb="3" color="gray">
              Upload a CSV with columns: Number, State, IsDirty (max 500 rows).
            </Text>

            <Box
              onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
              onDragLeave={() => setDragOver(false)}
              onDrop={(e) => {
                e.preventDefault();
                setDragOver(false);
                handleFile(e.dataTransfer.files[0]);
              }}
              onClick={() => inputRef.current?.click()}
              style={{
                border: `2px dashed var(${dragOver ? "--mint-9" : "--gray-6"})`,
                borderRadius: "var(--radius-3)",
                padding: "32px",
                textAlign: "center",
                cursor: "pointer",
                background: dragOver ? "var(--mint-a2)" : "var(--gray-a2)",
                transition: "all 150ms",
              }}
            >
              <input
                ref={inputRef}
                type="file"
                accept=".csv"
                style={{ display: "none" }}
                onChange={(e) => handleFile(e.target.files?.[0])}
              />
              {file ? (
                <>
                  <Text weight="bold">{file.name}</Text>
                  <Text size="1" color={file.size > MAX_FILE_SIZE ? "red" : "gray"} as="p">
                    {(file.size / 1024).toFixed(1)} KB
                    {file.size > MAX_FILE_SIZE && " — exceeds 100 KB limit"}
                  </Text>
                </>
              ) : (
                <Text color="gray">
                  Drag &amp; drop a .csv file here, or click to browse
                </Text>
              )}
            </Box>

            <Flex justify="end" gap="2" mt="4">
              <Dialog.Close>
                <Button variant="soft" color="gray">Cancel</Button>
              </Dialog.Close>
              <Button
                color="mint"
                disabled={!file || (file?.size ?? 0) > MAX_FILE_SIZE}
                onClick={handleUpload}
              >
                Import
              </Button>
            </Flex>
          </>
        )}

        {phase === "uploading" && (
          <Text color="gray" size="3">Uploading and processing...</Text>
        )}

        {phase === "done" && result && (
          <>
            <Flex gap="3" mb="3">
              <Badge color="green" size="2">{result.imported} imported</Badge>
              {result.errors.length > 0 && (
                <Badge color="red" size="2">{result.errors.length} error{result.errors.length !== 1 ? "s" : ""}</Badge>
              )}
            </Flex>

            {result.errors.length > 0 && (
              <Box
                mb="3"
                p="3"
                style={{
                  background: "var(--red-a2)",
                  borderRadius: "var(--radius-2)",
                  maxHeight: "150px",
                  overflowY: "auto",
                }}
              >
                {result.errors.map((e, i) => (
                  <Text key={i} size="2" as="p" color="red">
                    Row {e.row}: {e.message}
                  </Text>
                ))}
              </Box>
            )}

            <Flex justify="end" mt="4">
              <Button color="gray" variant="soft" onClick={handleClose}>
                Close
              </Button>
            </Flex>
          </>
        )}
      </Dialog.Content>
    </Dialog.Root>
  );
}
