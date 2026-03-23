import { Text, Card, Inset, Dialog } from "@radix-ui/themes";
import { PropsWithChildren } from "react";
import styled from "styled-components";

/** 600px wide image for Rooms */
const RoomImg = styled.img`
  width: 100%;
  height: 200px;
  object-fit: cover;
  display: block;
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
      <Card size="2" variant="classic" asChild>
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
