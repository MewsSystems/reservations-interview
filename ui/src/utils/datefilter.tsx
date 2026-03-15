import { Flex, TextField } from "@radix-ui/themes";

interface Props {
    from?: string;
    to?: string;
    onFromChange: (v: string) => void;
    onToChange: (v: string) => void;
}

export function DateFilter({ from, to, onFromChange, onToChange }: Props) {
    return (
        <Flex gap="4" mt="4">
            <TextField.Root
                type="date"
                value={from ?? ""}
                onChange={(e) => onFromChange(e.target.value)}
            />

            <TextField.Root
                type="date"
                value={to ?? ""}
                onChange={(e) => onToChange(e.target.value)}
            />
        </Flex>
    );
}