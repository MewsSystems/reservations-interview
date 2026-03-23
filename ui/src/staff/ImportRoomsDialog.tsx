import { useCallback, useState } from "react";
import { Badge, Box, Button, Dialog, Flex, Separator, Table, Text } from "@radix-ui/themes";
import { useQueryClient } from "@tanstack/react-query";
import { importRooms, ImportResult } from "./api";
import { useAuth } from "../utils/auth";
import { useShowErrorToast, useShowSuccessToast } from "../utils/toasts";

interface ImportRoomsDialogProps {
  children: React.ReactNode;
}

export function ImportRoomsDialog({ children }: ImportRoomsDialogProps) {
  const { token } = useAuth();
  const queryClient = useQueryClient();
  const showError = useShowErrorToast();
  const showSuccess = useShowSuccessToast("Rooms imported successfully!");

  const [open, setOpen] = useState(false);
  const [dragging, setDragging] = useState(false);
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<ImportResult | null>(null);

  function reset() {
    setFile(null);
    setResult(null);
    setDragging(false);
    setLoading(false);
  }

  function handleClose() {
    reset();
    setOpen(false);
  }

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragging(false);
    const droppedFile = e.dataTransfer.files[0];
    if (droppedFile && droppedFile.name.endsWith(".csv")) {
      setFile(droppedFile);
      setResult(null);
    }
  }, []);

  function handleFileSelect(e: React.ChangeEvent<HTMLInputElement>) {
    const selected = e.target.files?.[0];
    if (selected) {
      setFile(selected);
      setResult(null);
    }
  }

  async function handleImport() {
    if (!file || !token) return;
    setLoading(true);
    try {
      const res = await importRooms(token, file);
      setResult(res);
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
      if (res.failed === 0) showSuccess();
    } catch {
      showError(["Failed to import rooms"]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <Dialog.Root open={open} onOpenChange={(o) => { setOpen(o); if (!o) reset(); }}>
      <Dialog.Trigger>{children}</Dialog.Trigger>
      <Dialog.Content size="3" style={{ maxWidth: 500 }}>
        <Dialog.Title>Import Rooms</Dialog.Title>
        <Dialog.Description>
          Upload a CSV file with room numbers (one per line).
        </Dialog.Description>
        <Separator color="cyan" size="4" my="4" />

        {!result ? (
          <>
            <Box
              onDragOver={(e) => { e.preventDefault(); setDragging(true); }}
              onDragLeave={() => setDragging(false)}
              onDrop={handleDrop}
              style={{
                border: `2px dashed ${dragging ? "var(--accent-9)" : "var(--gray-6)"}`,
                borderRadius: "var(--radius-3)",
                padding: "40px 20px",
                textAlign: "center",
                backgroundColor: dragging ? "var(--accent-3)" : "var(--gray-2)",
                cursor: "pointer",
                transition: "all 0.2s",
              }}
              onClick={() => document.getElementById("csv-file-input")?.click()}
            >
              <Text size="3" color="gray">
                {file ? file.name : "Drag & drop a CSV file here, or click to browse"}
              </Text>
              <input
                id="csv-file-input"
                type="file"
                accept=".csv"
                onChange={handleFileSelect}
                style={{ display: "none" }}
              />
            </Box>

            <Flex justify="end" gap="3" mt="4">
              <Dialog.Close>
                <Button variant="outline" color="gray">Cancel</Button>
              </Dialog.Close>
              <Button
                color="mint"
                disabled={!file || loading}
                onClick={handleImport}
              >
                {loading ? "Importing..." : "Import"}
              </Button>
            </Flex>
          </>
        ) : (
          <>
            <Flex gap="3" mb="4">
              <Badge size="2" color="green">{result.imported} imported</Badge>
              {result.failed > 0 && (
                <Badge size="2" color="red">{result.failed} failed</Badge>
              )}
            </Flex>

            {result.errors.length > 0 && (
              <Box style={{ maxHeight: 300, overflowY: "auto" }}>
              <Table.Root variant="surface" size="1">
                <Table.Header>
                  <Table.Row>
                    <Table.ColumnHeaderCell>Line</Table.ColumnHeaderCell>
                    <Table.ColumnHeaderCell>Room</Table.ColumnHeaderCell>
                    <Table.ColumnHeaderCell>Error</Table.ColumnHeaderCell>
                  </Table.Row>
                </Table.Header>
                <Table.Body>
                  {result.errors.map((err, i) => (
                    <Table.Row key={i}>
                      <Table.Cell>{err.line}</Table.Cell>
                      <Table.Cell>{err.roomNumber}</Table.Cell>
                      <Table.Cell>{err.message}</Table.Cell>
                    </Table.Row>
                  ))}
                </Table.Body>
              </Table.Root>
              </Box>
            )}

            <Flex justify="end" gap="3" mt="4">
              <Button variant="outline" color="gray" onClick={reset}>
                Import Another
              </Button>
              <Button color="mint" onClick={handleClose}>
                Done
              </Button>
            </Flex>
          </>
        )}
      </Dialog.Content>
    </Dialog.Root>
  );
}
