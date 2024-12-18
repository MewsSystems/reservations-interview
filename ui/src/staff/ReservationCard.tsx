import { Text, Card } from "@radix-ui/themes";
import { PropsWithChildren } from "react";

export type ReservationCardProps = PropsWithChildren<{
  roomNumber: string;
  start: string;
  end: string;
  guestEmail: string;
}>;

export function ReservationCard(props: ReservationCardProps) {
  return (
    <Card size="3" variant="classic">
      <Text size="5" align="left">
        Room #{props.roomNumber}<br />
        Start: {props.start}<br />
        End: {props.end}<br />
        Guest email: {props.guestEmail}
      </Text>
    </Card>
  );
}
