import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/core';
import { Organisation, PostalAddress, TelecommunicationsNumber } from '@allors/domain/generated';
import { Database } from '@allors/workspace/core';

export function extendOrganisation(database: Database) {
  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.Organisation);
  assert(cls);

  Object.defineProperties(cls.prototype, {
    displayName: {
      configurable: true,
      get(this: Organisation): string {
        return this.Name || 'N/A';
      },
    },
    displayClassification: {
      configurable: true,
      get(this: Organisation): string {
        return this.CustomClassifications.map((w) => w.Name).join(', ');
      },
    },
    displayAddress: {
      configurable: true,
      get(this: Organisation): string {
        if (
          this.GeneralCorrespondence &&
          this.GeneralCorrespondence.objectType.name === 'PostalAddress'
        ) {
          const postalAddress = this.GeneralCorrespondence as PostalAddress;
          return `${postalAddress.Address1 ? postalAddress.Address1 : ''} ${
            postalAddress.Address2 ? postalAddress.Address2 : ''
          } ${postalAddress.Address3 ? postalAddress.Address3 : ''}`;
        }

        return '';
      },
    },
    displayAddress2: {
      configurable: true,
      get(this: Organisation): string {
        if (
          this.GeneralCorrespondence &&
          this.GeneralCorrespondence.objectType.name === 'PostalAddress'
        ) {
          const postalAddress = this.GeneralCorrespondence as PostalAddress;
          return `${postalAddress.PostalCode} ${postalAddress.Locality}`;
        }

        return '';
      },
    },
    displayAddress3: {
      configurable: true,
      get(this: Organisation): string {
        if (
          this.GeneralCorrespondence &&
          this.GeneralCorrespondence.objectType.name === 'PostalAddress'
        ) {
          const postalAddress = this.GeneralCorrespondence as PostalAddress;
          return `${postalAddress.Country ? postalAddress.Country.Name : ''}`;
        }

        return '';
      },
    },
    displayPhone: {
      configurable: true,
      get(this: Organisation): string {
        const telecommunicationsNumbers = this.PartyContactMechanisms.filter(
          (v) => v.ContactMechanism?.objectType === m.TelecommunicationsNumber
        );

        if (telecommunicationsNumbers.length > 0) {
          return telecommunicationsNumbers
            .map((v) => {
              const telecommunicationsNumber = v.ContactMechanism as TelecommunicationsNumber;
              return telecommunicationsNumber.displayName;
            })
            .reduce((acc: string, cur: string) => acc + ', ' + cur);
        }

        return '';
      },
    },
  });
}
