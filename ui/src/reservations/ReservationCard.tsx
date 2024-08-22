import { Text, Card, Inset, Dialog } from "@radix-ui/themes";
import { PropsWithChildren } from "react";
import styled from "styled-components";

/** 600px wide image for Rooms */
const RoomImg = styled.img`
  min-width: 300px;
  width: 100%;
  max-width: 700px;
  height: auto;
`;

export type ReservationCardProps = PropsWithChildren<{
  onClick: () => void;
  imgSrc: string;
  roomNumber: string;
}>;

/** A Card wrapped in a Dialog.Trigger */
export function ReservationCard(props: ReservationCardProps) {
  return (
    <Dialog.Trigger>
      <Card size="3" variant="classic" asChild>
        <a href="#" onClick={props.onClick}>
          <Inset clip="padding-box" side="top" pb="current">
            <RoomImg src={props.imgSrc} alt="room photo" />
          </Inset>
          <Text size="5" align="left">
            Room #{props.roomNumber}
          </Text>
        </a>
      </Card>
    </Dialog.Trigger>
  );
}
