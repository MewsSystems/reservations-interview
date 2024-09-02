import { Text, Card, Inset, Dialog, Box } from "@radix-ui/themes";
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
  title: string;
  subTitle?: string;
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
            {props.title}
          </Text>

          {props.subTitle ?
            <Box>
              <Text size="3">
                {props.subTitle}
              </Text>
            </Box> : <></>}

        </a>
      </Card>
    </Dialog.Trigger>
  );
}
