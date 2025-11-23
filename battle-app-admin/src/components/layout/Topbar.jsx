import { Flex, Heading } from '@chakra-ui/react';
import { UserProfile } from '../auth/UserProfile';
import './Topbar.css';

export default function Topbar() {
  return (
    <Flex className="topbar">
      <div className="topbar-brand">
        <div className="topbar-icon">⚔️</div>
        <Heading size="md" className="topbar-title">Panel Administratora</Heading>
      </div>
      <UserProfile />
    </Flex>
  );
}
