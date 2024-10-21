import { Card, Flex, Heading, Inset } from "@radix-ui/themes";
import { Link } from "@tanstack/react-router";

export function LandingPage() {
  return (
    <Flex direction="row" align="center" justify="center" gap="9" pt="9">
      <Card size="3" asChild variant="classic">
        <Link to="/login" preload="intent">
          <Inset side="top" pb="current">
            <img
              src="https://images.unsplash.com/photo-1550527882-b71dea5f8089?q=80&w=240&h=360&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
              alt="Key on wood board"
              style={{
                width: 240,
                height: 360,
              }}
            />
          </Inset>
          <Heading align="center">Login</Heading>
        </Link>
      </Card>
      <Card size="3" asChild variant="classic">
        <Link to="/reservations" preload="intent">
          <Inset clip="padding-box" side="top" pb="current">
            <img
              src="https://images.unsplash.com/photo-1531576788337-610fa9c67107?q=80&w=240&h=360&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
              alt="Clean Bed"
              style={{
                width: 240,
                height: 360,
              }}
            />
          </Inset>
          <Heading align="center">Reserve</Heading>
        </Link>
      </Card>
    </Flex>
  );
}
