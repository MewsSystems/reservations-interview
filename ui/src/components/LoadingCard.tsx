import { Skeleton, Card } from "@radix-ui/themes";

export function LoadingCard() {
  return (
    <Card size="3" variant="classic">
      <Skeleton loading />
    </Card>
  );
}
