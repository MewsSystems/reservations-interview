import { Link } from "@tanstack/react-router";
import { Card, CardBody, CardFooter, Grid, Heading, Text } from "grommet";

/** No Operation, grommet looks at onClick to add styles */
function noop() {}

function handleLogin() {
  // TODO have a staff view
  alert("Not implemented");
}

export function LandingPage() {
  return (
    <Grid columns={"1/2"} gap={"2em"}>
      <Card onClick={handleLogin} background="graph-1">
        <CardBody>
          <Heading>Login</Heading>
        </CardBody>
        <CardFooter>
          <Text size="small">Staff View</Text>
        </CardFooter>
      </Card>
      <Link to="/reservations" preload="intent">
        <Card onClick={noop} background="graph-3">
          <CardBody>
            <Heading>Reserve a Room</Heading>
          </CardBody>
          <CardFooter>
            <Text size="small">Guest View</Text>
          </CardFooter>
        </Card>
      </Link>
    </Grid>
  );
}
